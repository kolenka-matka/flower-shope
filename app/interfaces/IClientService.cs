using FlowerShop.dto.request;
using FlowerShop.model;

namespace FlowerShop.interfaces;

public interface IClientService
{
    Task<IReadOnlyList<Client>> GetAllAsync();
    Task<Client?> GetByIdAsync(Guid id);
    Task<Client> AddAsync(CreateClientRequest request);
    Task<Client?> UpdateAsync(Guid id, UpdateClientRequest request);
    Task<bool> DeleteAsync(Guid id);
}
