namespace FlowerShop.dto.response;

public record PromocodeResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public int DiscountPercent { get; init; }
    public DateTime ValidUntil { get; init; }
    public int UsageLimit { get; init; }
    public int UsedCount { get; init; }
    public bool IsActive { get; init; }
    public bool IsVipOnly { get; init; }
}
