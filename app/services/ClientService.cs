using FlowerShop.database;
using FlowerShop.dto.request;
using FlowerShop.interfaces;
using FlowerShop.model;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.services;

public class ClientService(FlowerShopDbContext db) : IClientService
{
    public async Task<IReadOnlyList<Client>> GetAllAsync()
        => await db.Clients
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .ToListAsync();

    public async Task<Client?> GetByIdAsync(Guid id)
        => await db.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Client> AddAsync(CreateClientRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("Имя клиента не должно быть пустым.");
        }

        if (request.LoyaltyPoints < 0)
        {
            throw new ArgumentException("Бонусный баланс не может быть отрицательным.");
        }

        var id = request.Id ?? Guid.NewGuid();
        if (await db.Clients.AnyAsync(x => x.Id == id))
        {
            throw new InvalidOperationException($"Клиент с идентификатором {id} уже существует.");
        }

        var entity = new Client
        {
            Id = id,
            FullName = request.FullName.Trim(),
            Phone = request.Phone.Trim(),
            Address = request.Address.Trim(),
            LoyaltyPoints = request.LoyaltyPoints,
            TotalSpent = 0m,
            Tier = LoyaltyTier.Bronze
        };

        db.Clients.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<Client?> UpdateAsync(Guid id, UpdateClientRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("Имя клиента не должно быть пустым.");
        }

        var entity = await db.Clients.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return null;
        }

        entity.FullName = request.FullName.Trim();
        entity.Phone = request.Phone.Trim();
        entity.Address = request.Address.Trim();
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await db.Clients.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            return false;
        }

        var hasOrders = await db.Orders.AnyAsync(o => o.ClientId == id);
        if (hasOrders)
        {
            throw new InvalidOperationException(
                "Нельзя удалить клиента: у него есть заказы.");
        }

        db.Clients.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }
}
