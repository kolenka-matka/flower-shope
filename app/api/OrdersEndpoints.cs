using FlowerShop.dto;
using FlowerShop.dto.request;
using FlowerShop.dto.response;
using FlowerShop.interfaces;

namespace FlowerShop.api;

public static class OrdersEndpoints
{
    public static RouteGroupBuilder MapOrdersEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/orders").WithTags("Orders");

        group.MapGet("/", async (IOrderService orders, IMapper mapper) =>
            {
                var result = await orders.GetAllAsync();
                return Results.Ok(result.Select(mapper.Map));
            })
            .WithSummary("Получить список заказов")
            .Produces<IEnumerable<OrderResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{orderId:guid}",
                async (Guid orderId, IOrderService orders, IMapper mapper) =>
            {
                var order = await orders.GetByIdAsync(orderId);
                return order is null
                    ? Results.NotFound(new ErrorResponse { Message = "Заказ не найден." })
                    : Results.Ok(mapper.Map(order));
            })
            .WithSummary("Получить заказ по идентификатору")
            .Produces<OrderResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapGet("/{orderId:guid}/invoice",
                async (Guid orderId, IOrderService orders) =>
            {
                var receipt = await orders.BuildReceiptAsync(orderId);
                return receipt is null
                    ? Results.NotFound(new ErrorResponse { Message = "Заказ не найден." })
                    : Results.File(receipt.Content, receipt.ContentType, receipt.FileName);
            })
            .WithSummary("Скачать счёт по заказу")
            .WithDescription("Формирует текстовый счёт (.txt) с расшифровкой состава и итогов.")
            .Produces(StatusCodes.Status200OK, contentType: "text/plain")
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateOrderRequest body, IOrderService orders, IMapper mapper) =>
            {
                try
                {
                    var created = await orders.CreateAsync(body);
                    return Results.Created($"/api/orders/{created.Id}", mapper.Map(created));
                }
                catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Создать заказ")
            .WithDescription("Главная бизнес-операция: разворачивает букеты, списывает остатки, " +
                             "применяет промокод и бонусы, назначает курьера, обновляет уровень клиента.")
            .Produces<OrderResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("/{orderId:guid}/status",
                async (Guid orderId, ChangeOrderStatusRequest body, IOrderService orders, IMapper mapper) =>
            {
                try
                {
                    var updated = await orders.ChangeStatusAsync(orderId, body.Status);
                    return updated is null
                        ? Results.NotFound(new ErrorResponse { Message = "Заказ не найден." })
                        : Results.Ok(mapper.Map(updated));
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Сменить статус заказа")
            .WithDescription("Переводит заказ на следующий статус: Created → Assigned → InDelivery → Delivered.")
            .Produces<OrderResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/{orderId:guid}/cancel",
                async (Guid orderId, IOrderService orders) =>
            {
                try
                {
                    var cancelled = await orders.CancelAsync(orderId);
                    return cancelled
                        ? Results.NoContent()
                        : Results.NotFound(new ErrorResponse { Message = "Заказ не найден." });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Отменить заказ")
            .WithDescription("Возвращает цветы на склад, освобождает курьера, возвращает бонусы и откатывает уровень.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        return api;
    }
}
