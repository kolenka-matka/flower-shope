using FlowerShop.model;

namespace FlowerShop.interfaces;

public interface IReceiptService
{
    ReceiptFile BuildReceipt(
        Order order,
        IReadOnlyDictionary<Guid, string> flowerNames,
        IReadOnlyDictionary<Guid, string> bouquetNames);
}
