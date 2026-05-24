namespace FlowerShop.model;

public class Client
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int LoyaltyPoints { get; set; }
    public decimal TotalSpent { get; set; }
    public LoyaltyTier Tier { get; set; } = LoyaltyTier.Bronze;
}

public enum LoyaltyTier
{
    Bronze = 0,
    Silver = 1,
    Gold = 2,
    Platinum = 3
}