using FlowerShop.dto.request;
using FlowerShop.model;

namespace FlowerShop.interfaces;

public interface ICourierService
{
    Task<IReadOnlyList<Courier>> GetAllAsync();
    Task<Courier?> GetByIdAsync(Guid id);
    Task<Courier> AddAsync(CreateCourierRequest request);
    Task<Courier?> UpdateAsync(Guid id, UpdateCourierRequest request);
    Task<bool> DeleteAsync(Guid id);
}
