using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Clients.Create;
using Application.Features.Clients.Delete;
using Application.Features.Clients.GetClientById;
using Application.Features.Clients.GetClients;
using Application.Features.Clients.History.ExportClientHistoryPdf;
using Application.Features.Clients.History.GetClientHistory;
using Application.Features.Clients.Update;
using Application.ReadModels.CRM;
using Application.ReadModels.Sales;
using Application.Security.Scopes;

namespace Api.Endpoints.Clients;

public sealed class ClientsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clients").WithTags("Clients");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<GetClientsQuery, IReadOnlyList<ClientReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetClientsQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ClientsRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetClientByIdQuery, ClientReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetClientByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ClientsRead);

        group
            .MapGet(
                "/{id:guid}/history",
                async (
                    Guid id,
                    DateTime? from,
                    DateTime? to,
                    string? status,
                    int? skip,
                    int? take,
                    IQueryHandler<
                        GetClientHistoryQuery,
                        IReadOnlyList<SalesOrderReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetClientHistoryQuery(id, from, to, status, skip ?? 0, take ?? 50),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ClientsRead);

        group
            .MapGet(
                "/{id:guid}/history/pdf",
                async (
                    Guid id,
                    DateTime? from,
                    DateTime? to,
                    string? status,
                    ICommandHandler<ExportClientHistoryPdfCommand, byte[]> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportClientHistoryPdfCommand(id, from, to, status),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value,
                            "application/pdf",
                            $"client_history_{id:N}.pdf"
                        );
                }
            )
            .RequireScope(SecurityScopes.ClientsRead);

        group
            .MapPost(
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
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/clients/{result.Value}",
                            new { clientId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.ClientsCreate);

        group
            .MapPut(
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
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.ClientsUpdate);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    ICommandHandler<DeleteClientCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new DeleteClientCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.ClientsDelete);
    }
}
