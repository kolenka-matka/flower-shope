using FlowerShop.model;

namespace FlowerShop.dto.request;

public record UpdateCourierRequest
{
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public int MaxActiveOrders { get; init; }
    public CourierStatus Status { get; init; }
}
