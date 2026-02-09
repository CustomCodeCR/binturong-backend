using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Purchases.PurchaseReceipts.Create;
using Application.Features.Purchases.PurchaseReceipts.GetPurchaseReceiptById;
using Application.Features.Purchases.PurchaseReceipts.GetPurchaseReceipts;
using Application.Features.Purchases.PurchaseReceipts.Reject;
using Application.ReadModels.Purchases;
using Application.Security.Scopes;

namespace Api.Endpoints.Purchases;

public sealed class PurchaseReceiptsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/purchases/receipts").WithTags("Purchases - Receipts");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<
                        GetPurchaseReceiptsQuery,
                        IReadOnlyList<PurchaseReceiptReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetPurchaseReceiptsQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PurchaseReceiptsRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetPurchaseReceiptByIdQuery, PurchaseReceiptReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetPurchaseReceiptByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PurchaseReceiptsRead);

        // HU-COM-02: register receipt (full/partial depends on lines vs order, handled in domain/handler)
        group
            .MapPost(
                "/",
                async (
                    CreatePurchaseReceiptRequest req,
                    ICommandHandler<CreatePurchaseReceiptCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreatePurchaseReceiptCommand(
                            req.PurchaseOrderId,
                            req.WarehouseId,
                            req.ReceiptDateUtc,
                            req.Notes,
                            req.Lines.Select(x => new CreatePurchaseReceiptLineDto(
                                    x.ProductId,
                                    x.QuantityReceived,
                                    x.UnitCost
                                ))
                                .ToList()
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/purchases/receipts/{result.Value}",
                            new { receiptId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.PurchaseReceiptsCreate);

        // HU-COM-02 scenario 3: rejected receipt
        group
            .MapPost(
                "/{id:guid}/reject",
                async (
                    Guid id,
                    RejectPurchaseReceiptRequest req,
                    ICommandHandler<RejectPurchaseReceiptCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RejectPurchaseReceiptCommand(id, req.Reason, req.RejectedAtUtc),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.PurchaseReceiptsReject);
    }
}
