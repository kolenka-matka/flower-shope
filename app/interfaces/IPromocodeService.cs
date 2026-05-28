using FlowerShop.dto.request;
using FlowerShop.model;

namespace FlowerShop.interfaces;

public interface IPromocodeService
{
    Task<IReadOnlyList<Promocode>> GetAllAsync();
    Task<Promocode?> GetByIdAsync(Guid id);
    Task<Promocode?> GetByCodeAsync(string code);
    Task<Promocode> AddAsync(CreatePromocodeRequest request);
    Task<Promocode?> UpdateAsync(Guid id, UpdatePromocodeRequest request);
    Task<bool> DeleteAsync(Guid id);
}
