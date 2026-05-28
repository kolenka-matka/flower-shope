using FlowerShop.model;

namespace FlowerShop.dto.response;

public record OrderResponse
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public string ClientNameSnapshot { get; init; } = string.Empty;
    public Guid CourierId { get; init; }
    public Guid[] FlowerIds { get; init; } = [];
    public Guid[] BouquetIds { get; init; } = [];
    public string DeliveryAddress { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal DeliveryFee { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal Total { get; init; }
    public Guid? PromocodeId { get; init; }
    public int EarnedPoints { get; init; }
    public OrderStatus Status { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
