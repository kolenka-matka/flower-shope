using FlowerShop.dto;
using FlowerShop.dto.request;
using FlowerShop.dto.response;
using FlowerShop.interfaces;

namespace FlowerShop.api;

public static class PromocodesEndpoints
{
    public static RouteGroupBuilder MapPromocodesEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/promocodes").WithTags("Promocodes");

        group.MapGet("/", async (IPromocodeService promocodes, IMapper mapper) =>
            {
                var result = await promocodes.GetAllAsync();
                return Results.Ok(result.Select(mapper.Map));
            })
            .WithSummary("Получить список промокодов")
            .Produces<IEnumerable<PromocodeResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{promocodeId:guid}",
                async (Guid promocodeId, IPromocodeService promocodes, IMapper mapper) =>
            {
                var promocode = await promocodes.GetByIdAsync(promocodeId);
                return promocode is null
                    ? Results.NotFound(new ErrorResponse { Message = "Промокод не найден." })
                    : Results.Ok(mapper.Map(promocode));
            })
            .WithSummary("Получить промокод по идентификатору")
            .Produces<PromocodeResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        // Специальный endpoint: проверить промокод по его коду (для UI до оформления заказа).
        group.MapGet("/{code}/validate",
                async (string code, IPromocodeService promocodes, IMapper mapper) =>
            {
                var promocode = await promocodes.GetByCodeAsync(code.Trim().ToUpperInvariant());
                return promocode is null
                    ? Results.NotFound(new ErrorResponse { Message = "Промокод не найден." })
                    : Results.Ok(mapper.Map(promocode));
            })
            .WithSummary("Проверить промокод по коду")
            .Produces<PromocodeResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreatePromocodeRequest body, IPromocodeService promocodes, IMapper mapper) =>
            {
                try
                {
                    var created = await promocodes.AddAsync(body);
                    return Results.Created($"/api/promocodes/{created.Id}", mapper.Map(created));
                }
                catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Добавить промокод")
            .Produces<PromocodeResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("/{promocodeId:guid}",
                async (Guid promocodeId, UpdatePromocodeRequest body, IPromocodeService promocodes, IMapper mapper) =>
            {
                try
                {
                    var updated = await promocodes.UpdateAsync(promocodeId, body);
                    return updated is null
                        ? Results.NotFound(new ErrorResponse { Message = "Промокод не найден." })
                        : Results.Ok(mapper.Map(updated));
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Изменить промокод")
            .Produces<PromocodeResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{promocodeId:guid}",
                async (Guid promocodeId, IPromocodeService promocodes) =>
            {
                try
                {
                    var deleted = await promocodes.DeleteAsync(promocodeId);
                    return deleted
                        ? Results.NoContent()
                        : Results.NotFound(new ErrorResponse { Message = "Промокод не найден." });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Удалить промокод")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        return api;
    }
}
