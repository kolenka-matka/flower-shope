# Flower Shop API
Курсовой проект по C#. Backend-сервис доставки цветов на ASP.NET Core Minimal API, EF Core и PostgreSQL.

## Запуск
1. PostgreSQL в Docker:
   `docker run --name flower-postgres -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=flower_shop_dev -p 5432:5432 -d postgres:17`
2. `cd app`
3. `dotnet run`
4. Swagger: http://localhost:5210/swagger