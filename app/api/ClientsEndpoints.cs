using FlowerShop.dto;
using FlowerShop.dto.request;
using FlowerShop.dto.response;
using FlowerShop.interfaces;

namespace FlowerShop.api;

public static class ClientsEndpoints
{
    public static RouteGroupBuilder MapClientsEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/clients").WithTags("Clients");

        group.MapGet("/", async (IClientService clients, IMapper mapper) =>
            {
                var result = await clients.GetAllAsync();
                return Results.Ok(result.Select(mapper.Map));
            })
            .WithSummary("Получить список клиентов")
            .Produces<IEnumerable<ClientResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{clientId:guid}",
                async (Guid clientId, IClientService clients, IMapper mapper) =>
            {
                var client = await clients.GetByIdAsync(clientId);
                return client is null
                    ? Results.NotFound(new ErrorResponse { Message = "Клиент не найден." })
                    : Results.Ok(mapper.Map(client));
            })
            .WithSummary("Получить клиента по идентификатору")
            .Produces<ClientResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        // Специальный endpoint: только данные программы лояльности.
        group.MapGet("/{clientId:guid}/loyalty",
                async (Guid clientId, IClientService clients, IMapper mapper) =>
            {
                var client = await clients.GetByIdAsync(clientId);
                return client is null
                    ? Results.NotFound(new ErrorResponse { Message = "Клиент не найден." })
                    : Results.Ok(mapper.MapLoyalty(client));
            })
            .WithSummary("Получить данные лояльности клиента")
            .WithDescription("Возвращает баллы, накопленную сумму и уровень клиента.")
            .Produces<ClientLoyaltyResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateClientRequest body, IClientService clients, IMapper mapper) =>
            {
                try
                {
                    var created = await clients.AddAsync(body);
                    return Results.Created($"/api/clients/{created.Id}", mapper.Map(created));
                }
                catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Добавить клиента")
            .Produces<ClientResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPut("/{clientId:guid}",
                async (Guid clientId, UpdateClientRequest body, IClientService clients, IMapper mapper) =>
            {
                try
                {
                    var updated = await clients.UpdateAsync(clientId, body);
                    return updated is null
                        ? Results.NotFound(new ErrorResponse { Message = "Клиент не найден." })
                        : Results.Ok(mapper.Map(updated));
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Изменить контактные данные клиента")
            .Produces<ClientResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{clientId:guid}",
                async (Guid clientId, IClientService clients) =>
            {
                try
                {
                    var deleted = await clients.DeleteAsync(clientId);
                    return deleted
                        ? Results.NoContent()
                        : Results.NotFound(new ErrorResponse { Message = "Клиент не найден." });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new ErrorResponse { Message = ex.Message });
                }
            })
            .WithSummary("Удалить клиента")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        return api;
    }
}
