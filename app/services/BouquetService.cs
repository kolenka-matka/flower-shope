using FlowerShop.database;
using FlowerShop.dto.request;
using FlowerShop.interfaces;
using FlowerShop.model;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.services;

public class BouquetService(FlowerShopDbContext db) : IBouquetService
{
    public async Task<IReadOnlyList<Bouquet>> GetAllAsync()
        => await db.Bouquets
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

    public async Task<Bouquet?> GetByIdAsync(Guid id)
        => await db.Bouquets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Bouquet> AddAsync(CreateBouquetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Название букета не должно быть пустым.");
        }

        if (request.FlowerIds.Length == 0)
        {
            throw new ArgumentException("Букет должен содержать хотя бы один цветок.");
        }

        if (request.Markup < 0)
        {
            throw new ArgumentException("Наценка не может быть отрицательной.");
        }

        // Проверяем, что все цветы из состава реально существуют.
        var uniqueFlowerIds = request.FlowerIds.Distinct().ToList();
        var existingCount = await db.Flowers.CountAsync(f => uniqueFlowerIds.Contains(f.Id));
        if (existingCount != uniqueFlowerIds.Count)
        {
            throw new InvalidOperationException("В составе букета указан несуществующий цветок.");
        }

        var entity = new Bouquet
        {
            Id = request.Id ?? Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            FlowerIds = request.FlowerIds,
            Markup = request.Markup,
            IsActive = true
        };

        db.Bouquets.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<Bouquet?> UpdateAsync(Guid id, UpdateBouquetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Название букета не должно быть пустым.");
        }

        if (request.FlowerIds.Length == 0)
        {
            throw new ArgumentException("Букет должен содержать хотя бы один цветок.");
        }

        if (request.Markup < 0)
        {
            throw new ArgumentException("Наценка не может быть отрицательной.");
        }

        var entity = await db.Bouquets.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return null;
        }

        var uniqueFlowerIds = request.FlowerIds.Distinct().ToList();
        var existingCount = await db.Flowers.CountAsync(f => uniqueFlowerIds.Contains(f.Id));
        if (existingCount != uniqueFlowerIds.Count)
        {
            throw new InvalidOperationException("В составе букета указан несуществующий цветок.");
        }

        entity.Name = request.Name.Trim();
        entity.Description = request.Description.Trim();
        entity.FlowerIds = request.FlowerIds;
        entity.Markup = request.Markup;
        entity.IsActive = request.IsActive;
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await db.Bouquets.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return false;
        }

        var used = await db.Orders.AnyAsync(o => o.BouquetIds.Contains(id));
        if (used)
        {
            throw new InvalidOperationException(
                "Нельзя удалить букет: он есть в заказах. Деактивируйте его через обновление (IsActive = false).");
        }

        db.Bouquets.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }
}
