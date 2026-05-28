namespace FlowerShop.model;

public class Order
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string ClientNameSnapshot { get; set; } = string.Empty;
    public Guid CourierId { get; set; }
    public Guid[] FlowerIds { get; set; } = [];
    public Guid[] BouquetIds { get; set; } = [];
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public Guid? PromocodeId { get; set; }
    public int PointsSpent { get; set; }
    public int EarnedPoints { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public DateTime CreatedAtUtc { get; set; }
}

public enum OrderStatus
{
    Created = 0,
    Assigned = 1,
    InDelivery = 2,
    Delivered = 3,
    Cancelled = 4
}
