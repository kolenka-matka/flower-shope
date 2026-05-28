namespace FlowerShop.dto.request;

public record UpdatePromocodeRequest
{
    public int DiscountPercent { get; init; }
    public DateTime ValidUntil { get; init; }
    public int UsageLimit { get; init; }
    public bool IsActive { get; init; }
    public bool IsVipOnly { get; init; }
}
