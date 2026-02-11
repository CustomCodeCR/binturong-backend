using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.SalesOrders.Confirm;
using Application.Features.SalesOrders.ConvertFromQuote;
using Application.Features.SalesOrders.Create;
using Application.Features.SalesOrders.GetSalesOrderById;
using Application.Features.SalesOrders.GetSalesOrders;
using Application.ReadModels.Sales;
using Application.Security.Scopes;

namespace Api.Endpoints.SalesOrders;

public sealed class SalesOrdersEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sales-orders").WithTags("SalesOrders");

        // =========================
        // GET /api/sales-orders
        // =========================
        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<GetSalesOrdersQuery, IReadOnlyList<SalesOrderReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetSalesOrdersQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.SalesOrdersRead);

        // =========================
        // GET /api/sales-orders/{id}
        // =========================
        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetSalesOrderByIdQuery, SalesOrderReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetSalesOrderByIdQuery(id), ct);

                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.SalesOrdersRead);

        // =========================
        // POST /api/sales-orders
        // =========================
        group
            .MapPost(
                "/",
                async (
                    CreateSalesOrderRequest req,
                    ICommandHandler<CreateSalesOrderCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new CreateSalesOrderCommand(
                        req.ClientId,
                        req.BranchId,
                        req.Currency,
                        req.ExchangeRate,
                        req.Notes,
                        req.Lines.Select(l => new CreateSalesOrderLine(
                                l.ProductId,
                                l.Quantity,
                                l.UnitPrice,
                                l.DiscountPerc,
                                l.TaxPerc
                            ))
                            .ToList()
                    );

                    var result = await handler.Handle(cmd, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/sales-orders/{result.Value}",
                            new { salesOrderId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.SalesOrdersCreate);

        // =========================
        // POST /api/sales-orders/from-quote/{quoteId}
        // HU-VTA-01 Convert quote -> sales order
        // =========================
        group
            .MapPost(
                "/from-quote/{quoteId:guid}",
                async (
                    Guid quoteId,
                    ConvertQuoteToSalesOrderRequest req,
                    ICommandHandler<ConvertQuoteToSalesOrderCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new ConvertQuoteToSalesOrderCommand(
                        quoteId,
                        req.BranchId,
                        req.Currency,
                        req.ExchangeRate,
                        req.Notes
                    );

                    var result = await handler.Handle(cmd, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/sales-orders/{result.Value}",
                            new { salesOrderId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.SalesOrdersConvertFromQuote);

        // =========================
        // POST /api/sales-orders/{id}/confirm
        // HU-VTA-03 Scenario 3: commission trigger on confirmation
        // =========================
        group
            .MapPost(
                "/{id:guid}/confirm",
                async (
                    Guid id,
                    ConfirmSalesOrderRequest req,
                    ICommandHandler<ConfirmSalesOrderCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ConfirmSalesOrderCommand(id, req.SellerUserId),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.SalesOrdersConfirm);
    }
}
