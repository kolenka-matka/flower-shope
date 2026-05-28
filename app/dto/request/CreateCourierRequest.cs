namespace FlowerShop.dto.request;


public record CreateCourierRequest
{
    public Guid? Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public int MaxActiveOrders { get; init; } = 5;
}
