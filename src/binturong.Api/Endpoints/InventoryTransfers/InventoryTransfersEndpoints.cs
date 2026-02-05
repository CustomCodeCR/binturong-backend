using Application.Abstractions.Messaging;
using Application.Features.InventoryTransfers.Approve;
using Application.Features.InventoryTransfers.Confirm;
using Application.Features.InventoryTransfers.Create;
using Application.Features.InventoryTransfers.Delete;
using Application.Features.InventoryTransfers.GetInventoryTransferById;
using Application.Features.InventoryTransfers.GetInventoryTransfers;
using Application.Features.InventoryTransfers.Reject;
using Application.Features.InventoryTransfers.RequestReview;
using Application.ReadModels.Inventory;

namespace Api.Endpoints.InventoryTransfers;

public sealed class InventoryTransfersEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory-transfers").WithTags("InventoryTransfers");

        // =========================
        // GET list
        // /api/inventory-transfers?page=1&pageSize=50&search=...
        // =========================
        group.MapGet(
            "/",
            async (
                int? page,
                int? pageSize,
                string? search,
                IQueryHandler<
                    GetInventoryTransfersQuery,
                    IReadOnlyList<InventoryTransferReadModel>
                > handler,
                CancellationToken ct
            ) =>
            {
                var query = new GetInventoryTransfersQuery(page ?? 1, pageSize ?? 50, search);
                var result = await handler.Handle(query, ct);

                return result.IsFailure
                    ? Results.BadRequest(result.Error)
                    : Results.Ok(result.Value);
            }
        );

        // =========================
        // GET by id
        // /api/inventory-transfers/{id}
        // =========================
        group.MapGet(
            "/{id:guid}",
            async (
                Guid id,
                IQueryHandler<GetInventoryTransferByIdQuery, InventoryTransferReadModel> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(new GetInventoryTransferByIdQuery(id), ct);
                return result.IsFailure ? Results.NotFound(result.Error) : Results.Ok(result.Value);
            }
        );

        // =========================
        // CREATE (Draft)
        // POST /api/inventory-transfers
        // =========================
        group.MapPost(
            "/",
            async (
                CreateInventoryTransferRequest req,
                ICommandHandler<CreateInventoryTransferCommand, Guid> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new CreateInventoryTransferCommand(
                    req.FromBranchId,
                    req.ToBranchId,
                    req.Notes,
                    req.CreatedByUserId,
                    req.Lines.Select(l => new CreateInventoryTransferLineDto(
                            l.ProductId,
                            l.Quantity,
                            l.FromWarehouseId,
                            l.ToWarehouseId
                        ))
                        .ToList()
                );

                var result = await handler.Handle(cmd, ct);

                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Created(
                    $"/api/inventory-transfers/{result.Value}",
                    new { transferId = result.Value }
                );
            }
        );

        // =========================
        // REQUEST REVIEW (PendingReview)
        // POST /api/inventory-transfers/{id}/request-review
        // =========================
        group.MapPost(
            "/{id:guid}/request-review",
            async (
                Guid id,
                ICommandHandler<RequestInventoryTransferReviewCommand> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(
                    new RequestInventoryTransferReviewCommand(id),
                    ct
                );
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        // =========================
        // APPROVE
        // POST /api/inventory-transfers/{id}/approve
        // =========================
        group.MapPost(
            "/{id:guid}/approve",
            async (
                Guid id,
                ApproveInventoryTransferRequest req,
                ICommandHandler<ApproveInventoryTransferCommand> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(
                    new ApproveInventoryTransferCommand(id, req.ApprovedByUserId),
                    ct
                );
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        // =========================
        // REJECT
        // POST /api/inventory-transfers/{id}/reject
        // =========================
        group.MapPost(
            "/{id:guid}/reject",
            async (
                Guid id,
                RejectInventoryTransferRequest req,
                ICommandHandler<RejectInventoryTransferCommand> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(
                    new RejectInventoryTransferCommand(id, req.RejectedByUserId, req.Reason),
                    ct
                );
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        // =========================
        // CONFIRM (executes stock movement)
        // POST /api/inventory-transfers/{id}/confirm
        // =========================
        group.MapPost(
            "/{id:guid}/confirm",
            async (
                Guid id,
                ConfirmInventoryTransferRequest req,
                ICommandHandler<ConfirmInventoryTransferCommand> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(
                    new ConfirmInventoryTransferCommand(id, req.RequireApproval),
                    ct
                );
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        // =========================
        // DELETE
        // DELETE /api/inventory-transfers/{id}
        // =========================
        group.MapDelete(
            "/{id:guid}",
            async (
                Guid id,
                ICommandHandler<DeleteInventoryTransferCommand> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(new DeleteInventoryTransferCommand(id), ct);
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );
    }
}
