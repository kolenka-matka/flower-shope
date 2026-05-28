using FlowerShop.dto;
using FlowerShop.dto.request;
using FlowerShop.dto.response;
using FlowerShop.interfaces;

namespace FlowerShop.api;

public static class FlowersEndpoints
{
    public static RouteGroupBuilder MapFlowersEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/flowers").WithTags("Flowers");

        group.MapGet("/", async (IFlowerService flowers, IMapper mapper) =>
            {
                var result = await flowers.GetAllAsync();
                return Results.Ok(result.Select(mapper.Map));
            })
            .WithSummary("Получить список цветов")
            .Produces<IEnumerable<FlowerResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{flowerId:guid}",
                async (Guid flowerId, IFlowerService flowers, IMapper mapper) =>
            {
                var flower = await flowers.GetByIdAsync(flowerId);
                return flower is null
                    ? Results.NotFound(new ErrorResponse { Message = "Цветок не найден." })
                    : Results.Ok(mapper.Map(flower));
            })
            .WithSummary("Получить цветок по идентификатору")
            .Produces<FlowerResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateFlowerRequest body, IFlowerService flowers, IMapper mapper) =>
            {
                try
                {
                    var created = await flowers.AddAsync(body);
                    return Results.Created($"/api/flowers/{created.Id}", mapper.Map(created));
                }
                catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Добавить цветок")
            .Produces<FlowerResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("/{flowerId:guid}",
                async (Guid flowerId, UpdateFlowerRequest body, IFlowerService flowers, IMapper mapper) =>
            {
                try
                {
                    var updated = await flowers.UpdateAsync(flowerId, body);
                    return updated is null
                        ? Results.NotFound(new ErrorResponse { Message = "Цветок не найден." })
                        : Results.Ok(mapper.Map(updated));
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Изменить цветок")
            .Produces<FlowerResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{flowerId:guid}",
                async (Guid flowerId, IFlowerService flowers) =>
            {
                try
                {
                    var deleted = await flowers.DeleteAsync(flowerId);
                    return deleted
                        ? Results.NoContent()
                        : Results.NotFound(new ErrorResponse { Message = "Цветок не найден." });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Удалить цветок")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        return api;
    }
}
