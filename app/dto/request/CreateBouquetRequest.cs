namespace FlowerShop.dto.request;


public record CreateBouquetRequest
{
    public Guid? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid[] FlowerIds { get; init; } = [];
    public decimal Markup { get; init; }
}
