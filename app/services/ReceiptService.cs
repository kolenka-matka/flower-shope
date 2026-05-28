using System.Text;
using FlowerShop.interfaces;
using FlowerShop.model;

namespace FlowerShop.services;

public class ReceiptService : IReceiptService
{
    public ReceiptFile BuildReceipt(
        Order order,
        IReadOnlyDictionary<Guid, string> flowerNames,
        IReadOnlyDictionary<Guid, string> bouquetNames)
    {
        var builder = new StringBuilder();
        builder.AppendLine("FLOWER SHOP");
        builder.AppendLine("Счёт по заказу");
        builder.AppendLine(new string('-', 44));
        builder.AppendLine($"Заказ:  {order.Id}");
        builder.AppendLine($"Дата (UTC): {order.CreatedAtUtc:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"Клиент: {order.ClientNameSnapshot}");
        builder.AppendLine($"Адрес:  {order.DeliveryAddress}");
        builder.AppendLine($"Статус: {order.Status}");
        builder.AppendLine(new string('-', 44));

        if (order.FlowerIds.Length > 0)
        {
            builder.AppendLine("Цветы:");
            foreach (var group in order.FlowerIds.GroupBy(x => x))
            {
                var name = flowerNames.GetValueOrDefault(group.Key, group.Key.ToString());
                builder.AppendLine($"  - {name} x{group.Count()}");
            }
        }

        if (order.BouquetIds.Length > 0)
        {
            builder.AppendLine("Букеты:");
            foreach (var group in order.BouquetIds.GroupBy(x => x))
            {
                var name = bouquetNames.GetValueOrDefault(group.Key, group.Key.ToString());
                builder.AppendLine($"  - {name} x{group.Count()}");
            }
        }

        builder.AppendLine(new string('-', 44));
        builder.AppendLine($"Сумма товаров:  {order.Subtotal,12:F2} RUB");
        builder.AppendLine($"Скидка:         {order.DiscountAmount,12:F2} RUB");
        builder.AppendLine($"Доставка:       {order.DeliveryFee,12:F2} RUB");
        builder.AppendLine($"ИТОГО:          {order.Total,12:F2} RUB");
        builder.AppendLine(new string('-', 44));
        builder.AppendLine($"Списано бонусов:   {order.PointsSpent}");
        builder.AppendLine($"Начислено бонусов: {order.EarnedPoints}");
        builder.AppendLine("Спасибо за заказ!");

        return new ReceiptFile
        {
            FileName = $"invoice-{order.Id}.txt",
            ContentType = "text/plain; charset=utf-8",
            Content = Encoding.UTF8.GetBytes(builder.ToString())
        };
    }
}
