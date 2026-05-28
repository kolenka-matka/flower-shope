namespace FlowerShop.dto.request;

public record UpdateBouquetRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid[] FlowerIds { get; init; } = [];
    public decimal Markup { get; init; }
    public bool IsActive { get; init; }
}
