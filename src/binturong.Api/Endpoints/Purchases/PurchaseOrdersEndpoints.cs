using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Purchases.PurchaseOrders.Create;
using Application.Features.Purchases.PurchaseOrders.GetPurchaseOrderById;
using Application.Features.Purchases.PurchaseOrders.GetPurchaseOrders;
using Application.ReadModels.Purchases;
using Application.Security.Scopes;

namespace Api.Endpoints.Purchases;

public sealed class PurchaseOrdersEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/purchases/orders").WithTags("Purchases - Orders");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<
                        GetPurchaseOrdersQuery,
                        IReadOnlyList<PurchaseOrderReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetPurchaseOrdersQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PurchaseOrdersRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetPurchaseOrderByIdQuery, PurchaseOrderReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetPurchaseOrderByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PurchaseOrdersRead);

        group
            .MapPost(
                "/",
                async (
                    CreatePurchaseOrderRequest req,
                    ICommandHandler<CreatePurchaseOrderCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreatePurchaseOrderCommand(
                            req.Code,
                            req.SupplierId,
                            req.BranchId,
                            req.RequestId,
                            req.OrderDateUtc,
                            req.Currency,
                            req.ExchangeRate,
                            req.Lines.Select(x => new CreatePurchaseOrderLineDto(
                                    x.ProductId,
                                    x.Quantity,
                                    x.UnitPrice,
                                    x.DiscountPerc,
                                    x.TaxPerc
                                ))
                                .ToList()
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/purchases/orders/{result.Value}",
                            new { purchaseOrderId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.PurchaseOrdersCreate);
    }
}
