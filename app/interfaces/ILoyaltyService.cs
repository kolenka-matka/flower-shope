using FlowerShop.model;

namespace FlowerShop.interfaces;

public interface ILoyaltyService
{
    int CalculateEarnedPoints(decimal total, LoyaltyTier tier);

    LoyaltyTier CalculateTier(decimal totalSpent);

    void AddPoints(Client client, int points);
    void RemovePoints(Client client, int points);
}
