using FlowerShop.database;
using FlowerShop.dto.request;
using FlowerShop.interfaces;
using FlowerShop.model;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.services;

public class PromocodeService(FlowerShopDbContext db) : IPromocodeService
{
    public async Task<IReadOnlyList<Promocode>> GetAllAsync()
        => await db.Promocodes
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .ToListAsync();

    public async Task<Promocode?> GetByIdAsync(Guid id)
        => await db.Promocodes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Promocode?> GetByCodeAsync(string code)
        => await db.Promocodes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code);

    public async Task<Promocode> AddAsync(CreatePromocodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            throw new ArgumentException("Код промокода не должен быть пустым.");
        }

        if (request.DiscountPercent is < 1 or > 100)
        {
            throw new ArgumentException("Скидка должна быть в диапазоне от 1 до 100 процентов.");
        }

        if (request.UsageLimit <= 0)
        {
            throw new ArgumentException("Лимит использований должен быть положительным.");
        }

        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.Promocodes.AnyAsync(x => x.Code == code))
        {
            throw new InvalidOperationException($"Промокод {code} уже существует.");
        }

        var entity = new Promocode
        {
            Id = request.Id ?? Guid.NewGuid(),
            Code = code,
            DiscountPercent = request.DiscountPercent,
            ValidUntil = request.ValidUntil.ToUniversalTime(),
            UsageLimit = request.UsageLimit,
            UsedCount = 0,
            IsActive = true,
            IsVipOnly = request.IsVipOnly
        };

        db.Promocodes.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<Promocode?> UpdateAsync(Guid id, UpdatePromocodeRequest request)
    {
        if (request.DiscountPercent is < 1 or > 100)
        {
            throw new ArgumentException("Скидка должна быть в диапазоне от 1 до 100 процентов.");
        }

        if (request.UsageLimit <= 0)
        {
            throw new ArgumentException("Лимит использований должен быть положительным.");
        }

        var entity = await db.Promocodes.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return null;
        }

        entity.DiscountPercent = request.DiscountPercent;
        entity.ValidUntil = request.ValidUntil.ToUniversalTime();
        entity.UsageLimit = request.UsageLimit;
        entity.IsActive = request.IsActive;
        entity.IsVipOnly = request.IsVipOnly;
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await db.Promocodes.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return false;
        }

        var used = await db.Orders.AnyAsync(o => o.PromocodeId == id);
        if (used)
        {
            throw new InvalidOperationException(
                "Нельзя удалить промокод: он применён в заказах. Деактивируйте его через обновление (IsActive = false).");
        }

        db.Promocodes.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }
}
