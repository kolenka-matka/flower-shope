using FlowerShop.dto.response;
using FlowerShop.model;

namespace FlowerShop.dto;

public class Mapper : IMapper
{
    public FlowerResponse Map(Flower flower) => new()
    {
        Id = flower.Id,
        Name = flower.Name,
        Color = flower.Color,
        Price = flower.Price,
        Stock = flower.Stock
    };

    public ClientResponse Map(Client client) => new()
    {
        Id = client.Id,
        FullName = client.FullName,
        Phone = client.Phone,
        Address = client.Address,
        LoyaltyPoints = client.LoyaltyPoints,
        TotalSpent = client.TotalSpent,
        Tier = client.Tier
    };

    public ClientLoyaltyResponse MapLoyalty(Client client) => new()
    {
        Id = client.Id,
        FullName = client.FullName,
        LoyaltyPoints = client.LoyaltyPoints,
        TotalSpent = client.TotalSpent,
        Tier = client.Tier
    };

    public CourierResponse Map(Courier courier) => new()
    {
        Id = courier.Id,
        FullName = courier.FullName,
        Phone = courier.Phone,
        MaxActiveOrders = courier.MaxActiveOrders,
        Status = courier.Status
    };

    public PromocodeResponse Map(Promocode promocode) => new()
    {
        Id = promocode.Id,
        Code = promocode.Code,
        DiscountPercent = promocode.DiscountPercent,
        ValidUntil = promocode.ValidUntil,
        UsageLimit = promocode.UsageLimit,
        UsedCount = promocode.UsedCount,
        IsActive = promocode.IsActive,
        IsVipOnly = promocode.IsVipOnly
    };

    public BouquetResponse Map(Bouquet bouquet) => new()
    {
        Id = bouquet.Id,
        Name = bouquet.Name,
        Description = bouquet.Description,
        FlowerIds = bouquet.FlowerIds,
        Markup = bouquet.Markup,
        IsActive = bouquet.IsActive
    };

    public OrderResponse Map(Order order) => new()
    {
        Id = order.Id,
        ClientId = order.ClientId,
        ClientNameSnapshot = order.ClientNameSnapshot,
        CourierId = order.CourierId,
        FlowerIds = order.FlowerIds,
        BouquetIds = order.BouquetIds,
        DeliveryAddress = order.DeliveryAddress,
        Subtotal = order.Subtotal,
        DeliveryFee = order.DeliveryFee,
        DiscountAmount = order.DiscountAmount,
        Total = order.Total,
        PromocodeId = order.PromocodeId,
        PointsSpent = order.PointsSpent,
        EarnedPoints = order.EarnedPoints,
        Status = order.Status,
        CreatedAtUtc = order.CreatedAtUtc
    };
}
