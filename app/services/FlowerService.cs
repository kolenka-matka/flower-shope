using FlowerShop.database;
using FlowerShop.dto.request;
using FlowerShop.interfaces;
using FlowerShop.model;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.services;

public class FlowerService(FlowerShopDbContext db) : IFlowerService
{
    public async Task<IReadOnlyList<Flower>> GetAllAsync()
        => await db.Flowers
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

    public async Task<Flower?> GetByIdAsync(Guid id)
        => await db.Flowers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Flower> AddAsync(CreateFlowerRequest request)
    {
        ValidateFields(request.Name, request.Price, request.Stock);

        var id = request.Id ?? Guid.NewGuid();
        if (await db.Flowers.AnyAsync(x => x.Id == id))
        {
            throw new InvalidOperationException($"Цветок с идентификатором {id} уже существует.");
        }

        var entity = new Flower
        {
            Id = id,
            Name = request.Name.Trim(),
            Color = request.Color.Trim(),
            Price = request.Price,
            Stock = request.Stock
        };

        db.Flowers.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<Flower?> UpdateAsync(Guid id, UpdateFlowerRequest request)
    {
        ValidateFields(request.Name, request.Price, request.Stock);

        var entity = await db.Flowers.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return null;
        }

        entity.Name = request.Name.Trim();
        entity.Color = request.Color.Trim();
        entity.Price = request.Price;
        entity.Stock = request.Stock;
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await db.Flowers.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return false;
        }

        var usedInOrder = await db.Orders.AnyAsync(o => o.FlowerIds.Contains(id));
        var usedInBouquet = await db.Bouquets.AnyAsync(b => b.FlowerIds.Contains(id));
        if (usedInOrder || usedInBouquet)
        {
            throw new InvalidOperationException(
                "Нельзя удалить цветок: он используется в заказах или букетах.");
        }

        db.Flowers.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }

    private static void ValidateFields(string name, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название цветка не должно быть пустым.");
        }

        if (price < 0)
        {
            throw new ArgumentException("Цена не может быть отрицательной.");
        }

        if (stock < 0)
        {
            throw new ArgumentException("Остаток на складе не может быть отрицательным.");
        }
    }
}
