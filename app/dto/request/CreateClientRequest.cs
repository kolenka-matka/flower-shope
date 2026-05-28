namespace FlowerShop.dto.request;

public record CreateClientRequest
{
    public Guid? Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public int LoyaltyPoints { get; init; }
}
