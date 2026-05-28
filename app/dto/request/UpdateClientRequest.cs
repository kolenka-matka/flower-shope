namespace FlowerShop.dto.request;

public record UpdateClientRequest
{
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
}
