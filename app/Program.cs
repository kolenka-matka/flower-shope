using FlowerShop.api;
using FlowerShop.database;
using FlowerShop.dto;
using FlowerShop.interfaces;
using FlowerShop.services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// свага
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Flower Shop API",
        Version = "v1",
        Description = "Сервис доставки цветов: цветы, букеты, клиенты, курьеры, заказы, лояльность, промокоды."
    });
});

// бд
var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Не найдена строка подключения ConnectionStrings:Postgres.");

builder.Services.AddDbContext<FlowerShopDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// сервисы
builder.Services.AddScoped<IMapper, Mapper>();
builder.Services.AddScoped<IFlowerService, FlowerService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ICourierService, CourierService>();
builder.Services.AddScoped<IPromocodeService, PromocodeService>();
builder.Services.AddScoped<IBouquetService, BouquetService>();
builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<FlowerShopDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Flower Shop API v1");
        options.RoutePrefix = "swagger";
    });
}

// эндпоинты
var api = app.MapGroup("/api");
api.MapFlowersEndpoints();
api.MapBouquetsEndpoints();
api.MapClientsEndpoints();
api.MapCouriersEndpoints();
api.MapPromocodesEndpoints();
api.MapOrdersEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    message = "Flower Shop API работает. Откройте /api/flowers, /api/bouquets, /api/clients, /api/couriers, /api/promocodes, /api/orders."
}));

await app.RunAsync();
