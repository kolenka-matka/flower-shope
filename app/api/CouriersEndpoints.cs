using FlowerShop.dto;
using FlowerShop.dto.request;
using FlowerShop.dto.response;
using FlowerShop.interfaces;

namespace FlowerShop.api;

public static class CouriersEndpoints
{
    public static RouteGroupBuilder MapCouriersEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/couriers").WithTags("Couriers");

        group.MapGet("/", async (ICourierService couriers, IMapper mapper) =>
            {
                var result = await couriers.GetAllAsync();
                return Results.Ok(result.Select(mapper.Map));
            })
            .WithSummary("Получить список курьеров")
            .Produces<IEnumerable<CourierResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{courierId:guid}",
                async (Guid courierId, ICourierService couriers, IMapper mapper) =>
            {
                var courier = await couriers.GetByIdAsync(courierId);
                return courier is null
                    ? Results.NotFound(new ErrorResponse { Message = "Курьер не найден." })
                    : Results.Ok(mapper.Map(courier));
            })
            .WithSummary("Получить курьера по идентификатору")
            .Produces<CourierResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateCourierRequest body, ICourierService couriers, IMapper mapper) =>
            {
                try
                {
                    var created = await couriers.AddAsync(body);
                    return Results.Created($"/api/couriers/{created.Id}", mapper.Map(created));
                }
                catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Добавить курьера")
            .Produces<CourierResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("/{courierId:guid}",
                async (Guid courierId, UpdateCourierRequest body, ICourierService couriers, IMapper mapper) =>
            {
                try
                {
                    var updated = await couriers.UpdateAsync(courierId, body);
                    return updated is null
                        ? Results.NotFound(new ErrorResponse { Message = "Курьер не найден." })
                        : Results.Ok(mapper.Map(updated));
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Изменить курьера")
            .Produces<CourierResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{courierId:guid}",
                async (Guid courierId, ICourierService couriers) =>
            {
                try
                {
                    var deleted = await couriers.DeleteAsync(courierId);
                    return deleted
                        ? Results.NoContent()
                        : Results.NotFound(new ErrorResponse { Message = "Курьер не найден." });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Удалить курьера")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        return api;
    }
}
