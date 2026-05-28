using FlowerShop.dto.request;
using FlowerShop.model;

namespace FlowerShop.interfaces;

public interface IOrderService
{
    Task<IReadOnlyList<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order> CreateAsync(CreateOrderRequest request);
    Task<Order?> ChangeStatusAsync(Guid id, OrderStatus newStatus);
    Task<bool> CancelAsync(Guid id);
    Task<ReceiptFile?> BuildReceiptAsync(Guid id);
}
