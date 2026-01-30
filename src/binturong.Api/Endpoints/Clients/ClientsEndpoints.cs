using Application.Abstractions.Messaging;
using Application.Features.Clients.Create;
using Application.Features.Clients.Delete;
using Application.Features.Clients.GetClientById;
using Application.Features.Clients.GetClients;
using Application.Features.Clients.Update;
using Application.ReadModels.CRM;

namespace Api.Endpoints.Clients;

public sealed class ClientsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clients").WithTags("Clients");

        // =========================
        // GET list
        // /api/clients?page=1&pageSize=50&search=acme
        // =========================
        group.MapGet(
            "/",
            async (
                int? page,
                int? pageSize,
                string? search,
                IQueryHandler<GetClientsQuery, IReadOnlyList<ClientReadModel>> handler,
                CancellationToken ct
            ) =>
            {
                var query = new GetClientsQuery(page ?? 1, pageSize ?? 50, search);
                var result = await handler.Handle(query, ct);

                return result.IsFailure
                    ? Results.BadRequest(result.Error)
                    : Results.Ok(result.Value);
            }
        );

        // =========================
        // GET by id
        // /api/clients/{id}
        // =========================
        group.MapGet(
            "/{id:guid}",
            async (
                Guid id,
                IQueryHandler<GetClientByIdQuery, ClientReadModel> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(new GetClientByIdQuery(id), ct);
                return result.IsFailure ? Results.NotFound(result.Error) : Results.Ok(result.Value);
            }
        );

        // =========================
        // CREATE
        // POST /api/clients
        // =========================
        group.MapPost(
            "/",
            async (
                CreateClientRequest req,
                ICommandHandler<CreateClientCommand, Guid> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new CreateClientCommand(
                    req.PersonType,
                    req.IdentificationType,
                    req.Identification,
                    req.TradeName,
                    req.ContactName,
                    req.Email,
                    req.PrimaryPhone,
                    req.SecondaryPhone,
                    req.Industry,
                    req.ClientType,
                    req.Score,
                    req.IsActive
                );

                var result = await handler.Handle(cmd, ct);

                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Created(
                    $"/api/clients/{result.Value}",
                    new { clientId = result.Value }
                );
            }
        );

        // =========================
        // UPDATE
        // PUT /api/clients/{id}
        // =========================
        group.MapPut(
            "/{id:guid}",
            async (
                Guid id,
                UpdateClientRequest req,
                ICommandHandler<UpdateClientCommand> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new UpdateClientCommand(
                    id,
                    req.TradeName,
                    req.ContactName,
                    req.Email,
                    req.PrimaryPhone,
                    req.SecondaryPhone,
                    req.Industry,
                    req.ClientType,
                    req.Score,
                    req.IsActive
                );

                var result = await handler.Handle(cmd, ct);

                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        // =========================
        // DELETE
        // DELETE /api/clients/{id}
        // =========================
        group.MapDelete(
            "/{id:guid}",
            async (Guid id, ICommandHandler<DeleteClientCommand> handler, CancellationToken ct) =>
            {
                var result = await handler.Handle(new DeleteClientCommand(id), ct);
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );
    }
}
