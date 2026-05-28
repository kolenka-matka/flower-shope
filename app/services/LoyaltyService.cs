using FlowerShop.interfaces;
using FlowerShop.model;

namespace FlowerShop.services;

public class LoyaltyService : ILoyaltyService
{
    private const decimal RublesPerPoint = 100m; // + 1 бонус за каждые 100 рублей.

    public int CalculateEarnedPoints(decimal total, LoyaltyTier tier)
    {
        if (total <= 0)
        {
            return 0;
        }

        var basePoints = total / RublesPerPoint;
        var multiplier = GetMultiplier(tier);
        return (int)Math.Floor(basePoints * multiplier);
    }

    public LoyaltyTier CalculateTier(decimal totalSpent) => totalSpent switch
    {
        >= 200_000m => LoyaltyTier.Platinum,
        >= 50_000m => LoyaltyTier.Gold,
        >= 10_000m => LoyaltyTier.Silver,
        _ => LoyaltyTier.Bronze
    };

    public void AddPoints(Client client, int points)
    {
        if (points <= 0)
        {
            return;
        }

        client.LoyaltyPoints += points;
    }

    public void RemovePoints(Client client, int points)
    {
        if (points <= 0)
        {
            return;
        }

        client.LoyaltyPoints = Math.Max(0, client.LoyaltyPoints - points);
    }

    // Множитель начисления бонусов в зависимости от уровня.
    private static decimal GetMultiplier(LoyaltyTier tier) => tier switch
    {
        LoyaltyTier.Platinum => 3m,
        LoyaltyTier.Gold => 2m,
        LoyaltyTier.Silver => 1.5m,
        _ => 1m
    };
}
