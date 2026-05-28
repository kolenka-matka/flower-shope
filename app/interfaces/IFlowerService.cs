using FlowerShop.dto.request;
using FlowerShop.model;

namespace FlowerShop.interfaces;

public interface IFlowerService
{
    Task<IReadOnlyList<Flower>> GetAllAsync();
    Task<Flower?> GetByIdAsync(Guid id);
    Task<Flower> AddAsync(CreateFlowerRequest request);
    Task<Flower?> UpdateAsync(Guid id, UpdateFlowerRequest request);
    Task<bool> DeleteAsync(Guid id);
}
