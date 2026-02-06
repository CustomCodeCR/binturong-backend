using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Warehouses.Create;
using Application.Features.Warehouses.Delete;
using Application.Features.Warehouses.Update;
using Application.Security.Scopes;

namespace Api.Endpoints.Warehouses;

public sealed class WarehousesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/warehouses").WithTags("Warehouses");

        // CREATE
        group
            .MapPost(
                "/",
                async (
                    CreateWarehouseRequest req,
                    ICommandHandler<CreateWarehouseCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreateWarehouseCommand(
                            req.BranchId,
                            req.Code,
                            req.Name,
                            req.Description,
                            req.IsActive
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/warehouses/{result.Value}",
                            new { warehouseId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.WarehousesCreate);

        // UPDATE
        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    UpdateWarehouseRequest req,
                    ICommandHandler<UpdateWarehouseCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new UpdateWarehouseCommand(
                            id,
                            req.Code,
                            req.Name,
                            req.Description,
                            req.IsActive
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.WarehousesUpdate);

        // DELETE
        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    ICommandHandler<DeleteWarehouseCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new DeleteWarehouseCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.WarehousesDelete);
    }
}
