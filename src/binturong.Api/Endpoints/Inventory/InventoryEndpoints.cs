using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Inventory.Movements.RegisterPhysicalCountAdjustment;
using Application.Features.Inventory.Movements.RegisterPurchaseEntry;
using Application.Features.Inventory.Movements.RegisterServiceExit;
using Application.Security.Scopes;

namespace Api.Endpoints.Inventory;

public sealed class InventoryEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory").WithTags("Inventory");

        group
            .MapPost(
                "/movements/purchase-in",
                async (
                    RegisterPurchaseEntryRequest req,
                    ICommandHandler<RegisterPurchaseEntryCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new RegisterPurchaseEntryCommand(
                        req.ProductId,
                        req.WarehouseId,
                        req.Quantity,
                        req.UnitCost,
                        req.Notes,
                        req.SourceId
                    );
                    var result = await handler.Handle(cmd, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/inventory/movements/{result.Value}",
                            new { movementId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.InventoryMovementsCreate);

        group
            .MapPost(
                "/movements/service-out",
                async (
                    RegisterServiceExitRequest req,
                    ICommandHandler<RegisterServiceExitCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new RegisterServiceExitCommand(
                        req.ProductId,
                        req.WarehouseId,
                        req.Quantity,
                        req.UnitCost,
                        req.Notes,
                        req.SourceId
                    );
                    var result = await handler.Handle(cmd, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/inventory/movements/{result.Value}",
                            new { movementId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.InventoryMovementsCreate);

        group
            .MapPost(
                "/movements/physical-adjustment",
                async (
                    RegisterPhysicalCountAdjustmentRequest req,
                    ICommandHandler<RegisterPhysicalCountAdjustmentCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new RegisterPhysicalCountAdjustmentCommand(
                        req.ProductId,
                        req.WarehouseId,
                        req.CountedStock,
                        req.UnitCost,
                        req.Justification
                    );
                    var result = await handler.Handle(cmd, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/inventory/movements/{result.Value}",
                            new { movementId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.InventoryMovementsCreate);
    }
}
