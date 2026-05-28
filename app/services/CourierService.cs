using FlowerShop.database;
using FlowerShop.dto.request;
using FlowerShop.interfaces;
using FlowerShop.model;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.services;

public class CourierService(FlowerShopDbContext db) : ICourierService
{
    public async Task<IReadOnlyList<Courier>> GetAllAsync()
        => await db.Couriers
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .ToListAsync();

    public async Task<Courier?> GetByIdAsync(Guid id)
        => await db.Couriers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Courier> AddAsync(CreateCourierRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("Имя курьера не должно быть пустым.");
        }

        if (request.MaxActiveOrders <= 0)
        {
            throw new ArgumentException("Лимит активных заказов должен быть положительным.");
        }

        var id = request.Id ?? Guid.NewGuid();
        if (await db.Couriers.AnyAsync(x => x.Id == id))
        {
            throw new InvalidOperationException($"Курьер с идентификатором {id} уже существует.");
        }

        var entity = new Courier
        {
            Id = id,
            FullName = request.FullName.Trim(),
            Phone = request.Phone.Trim(),
            MaxActiveOrders = request.MaxActiveOrders,
            Status = CourierStatus.Active
        };

        db.Couriers.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<Courier?> UpdateAsync(Guid id, UpdateCourierRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("Имя курьера не должно быть пустым.");
        }

        if (request.MaxActiveOrders <= 0)
        {
            throw new ArgumentException("Лимит активных заказов должен быть положительным.");
        }

        var entity = await db.Couriers.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return null;
        }

        entity.FullName = request.FullName.Trim();
        entity.Phone = request.Phone.Trim();
        entity.MaxActiveOrders = request.MaxActiveOrders;
        entity.Status = request.Status;
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await db.Couriers.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return false;
        }

        var hasOrders = await db.Orders.AnyAsync(o => o.CourierId == id);
        if (hasOrders)
        {
            throw new InvalidOperationException(
                "Нельзя удалить курьера: за ним числятся заказы.");
        }

        db.Couriers.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }
}
