using FlowerShop.model;

namespace FlowerShop.dto.response;

public record CourierResponse
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public int MaxActiveOrders { get; init; }
    public CourierStatus Status { get; init; }
}
