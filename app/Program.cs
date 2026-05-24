using FlowerShop.database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Flower Shop API",
        Version = "v1",
        Description = "Сервис доставки цветов с программой лояльности и промокодами."
    });
});

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Не найдена строка подключения ConnectionStrings:Postgres.");

builder.Services.AddDbContext<FlowerShopDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

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

app.MapGet("/", () => Results.Ok(new
{
    message = "Flower Shop API работает. Откройте /api/flowers, /api/clients, /api/couriers, /api/orders, /api/promocodes."
}));

await app.RunAsync();