using FlowerShop.model;

namespace FlowerShop.dto.response;

public record ClientResponse
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public int LoyaltyPoints { get; init; }
    public decimal TotalSpent { get; init; }
    public LoyaltyTier Tier { get; init; }
}
