using FlowerShop.dto.response;
using FlowerShop.model;

namespace FlowerShop.dto;

public interface IMapper
{
    FlowerResponse Map(Flower flower);
    ClientResponse Map(Client client);
    ClientLoyaltyResponse MapLoyalty(Client client);
    CourierResponse Map(Courier courier);
    PromocodeResponse Map(Promocode promocode);
    BouquetResponse Map(Bouquet bouquet);
    OrderResponse Map(Order order);
}
