namespace FlowerShop.dto.request;

public record CreateOrderRequest
{
    public Guid ClientId { get; init; }
    public Guid[] FlowerIds { get; init; } = [];
    public Guid[] BouquetIds { get; init; } = [];
    public string? DeliveryAddress { get; init; }
    public string? PromocodeCode { get; init; }
    public int UsePoints { get; init; }
}
