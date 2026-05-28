namespace FlowerShop.dto.request;

public record UpdateFlowerRequest
{
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
}
