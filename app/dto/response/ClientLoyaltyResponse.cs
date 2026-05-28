using FlowerShop.model;

namespace FlowerShop.dto.response;

public record ClientLoyaltyResponse
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public int LoyaltyPoints { get; init; }
    public decimal TotalSpent { get; init; }
    public LoyaltyTier Tier { get; init; }
}
