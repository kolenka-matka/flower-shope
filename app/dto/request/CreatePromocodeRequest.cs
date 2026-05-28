namespace FlowerShop.dto.request;

public record CreatePromocodeRequest
{
    public Guid? Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public int DiscountPercent { get; init; }
    public DateTime ValidUntil { get; init; }
    public int UsageLimit { get; init; }
    public bool IsVipOnly { get; init; }
}
