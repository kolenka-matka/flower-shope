using FlowerShop.model;

namespace FlowerShop.dto.request;

public record ChangeOrderStatusRequest
{
    public OrderStatus Status { get; init; }
}
