using FlowerShop.dto;
using FlowerShop.dto.request;
using FlowerShop.dto.response;
using FlowerShop.interfaces;

namespace FlowerShop.api;

public static class BouquetsEndpoints
{
    public static RouteGroupBuilder MapBouquetsEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/bouquets").WithTags("Bouquets");

        group.MapGet("/", async (IBouquetService bouquets, IMapper mapper) =>
            {
                var result = await bouquets.GetAllAsync();
                return Results.Ok(result.Select(mapper.Map));
            })
            .WithSummary("Получить список букетов")
            .Produces<IEnumerable<BouquetResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{bouquetId:guid}",
                async (Guid bouquetId, IBouquetService bouquets, IMapper mapper) =>
            {
                var bouquet = await bouquets.GetByIdAsync(bouquetId);
                return bouquet is null
                    ? Results.NotFound(new ErrorResponse { Message = "Букет не найден." })
                    : Results.Ok(mapper.Map(bouquet));
            })
            .WithSummary("Получить букет по идентификатору")
            .Produces<BouquetResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateBouquetRequest body, IBouquetService bouquets, IMapper mapper) =>
            {
                try
                {
                    var created = await bouquets.AddAsync(body);
                    return Results.Created($"/api/bouquets/{created.Id}", mapper.Map(created));
                }
                catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Добавить букет")
            .Produces<BouquetResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("/{bouquetId:guid}",
                async (Guid bouquetId, UpdateBouquetRequest body, IBouquetService bouquets, IMapper mapper) =>
            {
                try
                {
                    var updated = await bouquets.UpdateAsync(bouquetId, body);
                    return updated is null
                        ? Results.NotFound(new ErrorResponse { Message = "Букет не найден." })
                        : Results.Ok(mapper.Map(updated));
                }
                catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Изменить букет")
            .Produces<BouquetResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{bouquetId:guid}",
                async (Guid bouquetId, IBouquetService bouquets) =>
            {
                try
                {
                    var deleted = await bouquets.DeleteAsync(bouquetId);
                    return deleted
                        ? Results.NoContent()
                        : Results.NotFound(new ErrorResponse { Message = "Букет не найден." });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Удалить букет")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        return api;
    }
}
