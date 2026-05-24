using FlowerShop.model;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.database;

public class FlowerShopDbContext(DbContextOptions<FlowerShopDbContext> options) : DbContext(options)
{
    public DbSet<Flower> Flowers { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Courier> Couriers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Promocode> Promocodes { get; set; }
    public DbSet<Bouquet> Bouquets { get; set; }
}