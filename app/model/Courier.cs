namespace FlowerShop.model;

public class Courier
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int MaxActiveOrders { get; set; } = 5;
    public CourierStatus Status { get; set; } = CourierStatus.Active;
}

public enum CourierStatus
{
    Active = 0,
    OnVacation = 1
}