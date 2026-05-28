using FlowerShop.dto.request;
using FlowerShop.model;

namespace FlowerShop.interfaces;

public interface IBouquetService
{
    Task<IReadOnlyList<Bouquet>> GetAllAsync();
    Task<Bouquet?> GetByIdAsync(Guid id);
    Task<Bouquet> AddAsync(CreateBouquetRequest request);
    Task<Bouquet?> UpdateAsync(Guid id, UpdateBouquetRequest request);
    Task<bool> DeleteAsync(Guid id);
}
