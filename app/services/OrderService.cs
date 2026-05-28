using FlowerShop.database;
using FlowerShop.dto.request;
using FlowerShop.interfaces;
using FlowerShop.model;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.services;

public class OrderService(
    FlowerShopDbContext db,
    ILoyaltyService loyaltyService,
    IReceiptService receiptService) : IOrderService
{
    private const decimal DeliveryFee = 300m; // стоимость доставки фиксированная

    private static readonly OrderStatus[] ActiveStatuses =
    [OrderStatus.Created, OrderStatus.Assigned, OrderStatus.InDelivery]; // статусы, при которых заказ считается активным (занимает слот курьера)


    public async Task<IReadOnlyList<Order>> GetAllAsync()
        => await db.Orders
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

    public async Task<Order?> GetByIdAsync(Guid id)
        => await db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Order> CreateAsync(CreateOrderRequest request)
    {
        // 1. в заказе должен быть хотя бы один цветок или букет
        if (request.FlowerIds.Length == 0 && request.BouquetIds.Length == 0)
        {
            throw new ArgumentException("Заказ должен содержать хотя бы один цветок или букет.");
        }

        // 2. клиент должен существовать
        var client = await db.Clients.FirstOrDefaultAsync(x => x.Id == request.ClientId);
        if (client is null)
        {
            throw new InvalidOperationException("Клиент не найден.");
        }

        // 3. загружаем и проверяем букеты из заказа
        var bouquetIdsDistinct = request.BouquetIds.Distinct().ToList();
        var bouquets = await db.Bouquets
            .Where(b => bouquetIdsDistinct.Contains(b.Id))
            .ToListAsync();

        foreach (var bouquetId in bouquetIdsDistinct)
        {
            var bouquet = bouquets.FirstOrDefault(b => b.Id == bouquetId);
            if (bouquet is null)
            {
                throw new InvalidOperationException($"Букет {bouquetId} не найден.");
            }

            if (!bouquet.IsActive)
            {
                throw new InvalidOperationException($"Букет \"{bouquet.Name}\" недоступен для заказа.");
            }
        }

        // 4. подсчет сколько каждого цветка нужно: отдельные цветы + состав всех букетов
        var flowerQuantities = new Dictionary<Guid, int>();
        foreach (var flowerId in request.FlowerIds)
        {
            flowerQuantities[flowerId] = flowerQuantities.GetValueOrDefault(flowerId) + 1;
        }

        foreach (var bouquetId in request.BouquetIds)
        {
            var bouquet = bouquets.First(b => b.Id == bouquetId);
            foreach (var flowerId in bouquet.FlowerIds)
            {
                flowerQuantities[flowerId] = flowerQuantities.GetValueOrDefault(flowerId) + 1;
            }
        }

        // 5. загружаем цветы, проверяем существование и достаточность остатков
        var flowerIds = flowerQuantities.Keys.ToList();
        var flowers = await db.Flowers
            .Where(f => flowerIds.Contains(f.Id))
            .ToListAsync();

        foreach (var pair in flowerQuantities)
        {
            var flower = flowers.FirstOrDefault(f => f.Id == pair.Key);
            if (flower is null)
            {
                throw new InvalidOperationException($"Цветок {pair.Key} не найден.");
            }

            if (flower.Stock < pair.Value)
            {
                throw new InvalidOperationException(
                    $"Недостаточно цветов \"{flower.Name}\": нужно {pair.Value}, доступно {flower.Stock}.");
            }
        }

        // 6. проверка промокода (VIP считается по ТЕКУЩЕМУ уровню клиента)
        Promocode? promocode = null;
        if (!string.IsNullOrWhiteSpace(request.PromocodeCode))
        {
            promocode = await ValidatePromocodeAsync(
                request.PromocodeCode.Trim().ToUpperInvariant(), client);
        }

        // 7. подсчет стоимости товаров (Subtotal)
        var flowerPrices = flowers.ToDictionary(f => f.Id, f => f.Price);
        decimal subtotal = request.FlowerIds.Sum(id => flowerPrices[id]);
        foreach (var bouquetId in request.BouquetIds)
        {
            var bouquet = bouquets.First(b => b.Id == bouquetId);
            var bouquetFlowersCost = bouquet.FlowerIds.Sum(id => flowerPrices[id]);
            subtotal += bouquetFlowersCost + bouquet.Markup;
        }

        // 8. подсчет скидок: промокод (процент от Subtotal) + бонусы (1 балл = 1 рубль)
        decimal promoDiscount = promocode is null
            ? 0m
            : Math.Round(subtotal * promocode.DiscountPercent / 100m, 2);

        int pointsToUse = Math.Clamp(request.UsePoints, 0, client.LoyaltyPoints);
        decimal maxPointsDiscount = Math.Max(0m, subtotal - promoDiscount);
        decimal pointsDiscount = Math.Min(pointsToUse, maxPointsDiscount);
        pointsToUse = (int)pointsDiscount; // реально списываем столько баллов, сколько ушло в скидку

        decimal discountAmount = promoDiscount + pointsDiscount;

        // 9. итоговая сумма = товары - скидки + доставка
        decimal total = subtotal - discountAmount + DeliveryFee;

        // 10. поиск  свободного курьера до любых изменений (если нет — ничего не списываем)
        var courier = await FindAvailableCourierAsync();
        if (courier is null)
        {
            throw new InvalidOperationException("Нет свободных курьеров. Попробуйте позже.");
        }

        // 11. списание цветов
        foreach (var pair in flowerQuantities)
        {
            var flower = flowers.First(f => f.Id == pair.Key);
            flower.Stock -= pair.Value;
        }

        // 12. списание бонусов клиента
        loyaltyService.RemovePoints(client, pointsToUse);

        // 13. увеличение счётчика использований промокода
        if (promocode is not null)
        {
            promocode.UsedCount += 1;
        }

        // 14. увеличение суммы трат клиента и пересчет уровеня лояльности
        client.TotalSpent += total;
        client.Tier = loyaltyService.CalculateTier(client.TotalSpent);

        // 15. начисление бонусов с учётом НОВОГО уровня
        var earnedPoints = loyaltyService.CalculateEarnedPoints(total, client.Tier);
        loyaltyService.AddPoints(client, earnedPoints);

        // 16. создание заказа
        var order = new Order
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            ClientNameSnapshot = client.FullName,
            CourierId = courier.Id,
            FlowerIds = request.FlowerIds,
            BouquetIds = request.BouquetIds,
            DeliveryAddress = string.IsNullOrWhiteSpace(request.DeliveryAddress)
                ? client.Address
                : request.DeliveryAddress.Trim(),
            Subtotal = subtotal,
            DeliveryFee = DeliveryFee,
            DiscountAmount = discountAmount,
            Total = total,
            PromocodeId = promocode?.Id,
            PointsSpent = pointsToUse,
            EarnedPoints = earnedPoints,
            Status = OrderStatus.Created,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Orders.Add(order);

        await db.SaveChangesAsync(); // все изменения (остатки, бонусы, уровень, промокод, новый заказ) сохраняются одной транзакцией. если что-то упало раньше - ниче не запишется

        return order;
    }

    public async Task<Order?> ChangeStatusAsync(Guid id, OrderStatus newStatus)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null)
        {
            return null;
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Нельзя менять статус отменённого заказа.");
        }

        if (order.Status == OrderStatus.Delivered)
        {
            throw new InvalidOperationException("Заказ уже доставлен, статус менять нельзя.");
        }

        // Created -> Assigned -> InDelivery -> Delivered
        var expectedNext = order.Status + 1; // проверка последовательности шагов
        if (newStatus != expectedNext)
        {
            throw new InvalidOperationException(
                $"Недопустимый переход: из статуса {order.Status} можно перейти только в {expectedNext}.");
        }

        order.Status = newStatus;
        await db.SaveChangesAsync();
        return order;
    }

    public async Task<bool> CancelAsync(Guid id) // отмена заказа
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null)
        {
            return false;
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Заказ уже отменён.");
        }

        if (order.Status == OrderStatus.Delivered)
        {
            throw new InvalidOperationException("Нельзя отменить уже доставленный заказ.");
        }


        await RestoreStockAsync(order); // возвращаем цветы на склад (отдельные + из состава букетов)

        // откат лояльности клиента
        var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == order.ClientId);
        if (client is not null)
        {
            loyaltyService.RemovePoints(client, order.EarnedPoints); // забираем начисленные бонусы
            loyaltyService.AddPoints(client, order.PointsSpent);     // возвращаем потраченные бонусы
            client.TotalSpent = Math.Max(0m, client.TotalSpent - order.Total);
            client.Tier = loyaltyService.CalculateTier(client.TotalSpent);
        }

        // откат промокода
        if (order.PromocodeId is not null)
        {
            var promocode = await db.Promocodes.FirstOrDefaultAsync(p => p.Id == order.PromocodeId);
            if (promocode is not null && promocode.UsedCount > 0)
            {
                promocode.UsedCount -= 1;
            }
        }

        order.Status = OrderStatus.Cancelled; // курьер освобождается автоматически: его загрузка считается по активным заказам, а этот заказ становится Cancelled.
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<ReceiptFile?> BuildReceiptAsync(Guid id)
    {
        var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        if (order is null)
        {
            return null;
        }

        var flowerIds = order.FlowerIds.Distinct().ToList();
        var flowerNames = await db.Flowers
            .Where(f => flowerIds.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, f => f.Name);

        var bouquetIds = order.BouquetIds.Distinct().ToList();
        var bouquetNames = await db.Bouquets
            .Where(b => bouquetIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Name);

        return receiptService.BuildReceipt(order, flowerNames, bouquetNames);
    }

    // доп методы -------------------------------------------------------------------------------------------------------

    /// проверяет промокод и возвращает отслеживаемую сущность, чтобы можно было увеличить счётчик использований
    private async Task<Promocode> ValidatePromocodeAsync(string code, Client client)
    {
        var promocode = await db.Promocodes.FirstOrDefaultAsync(p => p.Code == code);
        if (promocode is null)
        {
            throw new InvalidOperationException($"Промокод {code} не найден.");
        }

        if (!promocode.IsActive)
        {
            throw new InvalidOperationException($"Промокод {code} неактивен.");
        }

        if (promocode.ValidUntil < DateTime.UtcNow)
        {
            throw new InvalidOperationException($"Срок действия промокода {code} истёк.");
        }

        if (promocode.UsedCount >= promocode.UsageLimit)
        {
            throw new InvalidOperationException($"Лимит использований промокода {code} исчерпан.");
        }

        if (promocode.IsVipOnly && client.Tier < LoyaltyTier.Gold)
        {
            throw new InvalidOperationException(
                $"Промокод {code} доступен только клиентам уровня Gold и выше.");
        }

        return promocode;
    }

    // находит наименее загруженного курьера, у которого есть свободный слот
    private async Task<Courier?> FindAvailableCourierAsync()
    {
        var couriers = await db.Couriers
            .Where(c => c.Status == CourierStatus.Active)
            .ToListAsync();

        // Считаем текущую загрузку каждого курьера одним запросом.
        var loads = await db.Orders
            .Where(o => ActiveStatuses.Contains(o.Status))
            .GroupBy(o => o.CourierId)
            .Select(g => new { CourierId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourierId, x => x.Count);

        return couriers
            .Select(c => new { Courier = c, Load = loads.GetValueOrDefault(c.Id, 0) })
            .Where(x => x.Load < x.Courier.MaxActiveOrders)
            .OrderBy(x => x.Load)
            .Select(x => x.Courier)
            .FirstOrDefault();
    }

    // Возвращает на склад все цветы заказа (отдельные + из состава букетов)
    private async Task RestoreStockAsync(Order order)
    {
        var flowerQuantities = new Dictionary<Guid, int>();
        foreach (var flowerId in order.FlowerIds)
        {
            flowerQuantities[flowerId] = flowerQuantities.GetValueOrDefault(flowerId) + 1;
        }

        if (order.BouquetIds.Length > 0)
        {
            var bouquetIds = order.BouquetIds.Distinct().ToList();
            var bouquets = await db.Bouquets
                .Where(b => bouquetIds.Contains(b.Id))
                .ToListAsync();

            foreach (var bouquetId in order.BouquetIds)
            {
                var bouquet = bouquets.FirstOrDefault(b => b.Id == bouquetId);
                if (bouquet is null)
                {
                    continue;
                }

                foreach (var flowerId in bouquet.FlowerIds)
                {
                    flowerQuantities[flowerId] = flowerQuantities.GetValueOrDefault(flowerId) + 1;
                }
            }
        }

        var flowerIds = flowerQuantities.Keys.ToList();
        var flowers = await db.Flowers
            .Where(f => flowerIds.Contains(f.Id))
            .ToListAsync();

        foreach (var pair in flowerQuantities)
        {
            var flower = flowers.FirstOrDefault(f => f.Id == pair.Key);
            if (flower is not null)
            {
                flower.Stock += pair.Value;
            }
        }
    }
}
