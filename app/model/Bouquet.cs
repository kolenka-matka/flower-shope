namespace FlowerShop.model;

public class Bouquet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid[] FlowerIds { get; set; } = [];
    public decimal Markup { get; set; }
    public bool IsActive { get; set; } = true;
}