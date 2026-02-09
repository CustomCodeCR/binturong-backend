using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.SupplierQuotes.Create;
using Application.Features.SupplierQuotes.Reject;
using Application.Features.SupplierQuotes.Respond;
using Application.Security.Scopes;

namespace Api.Endpoints.Suppliers;

public sealed class SupplierQuotesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/suppliers/quotes").WithTags("Suppliers - Quotes");

        group
            .MapPost(
                "/",
                async (
                    CreateSupplierQuoteRequest req,
                    ICommandHandler<CreateSupplierQuoteCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreateSupplierQuoteCommand(
                            req.Code,
                            req.SupplierId,
                            req.BranchId,
                            req.RequestedAtUtc,
                            req.Notes,
                            req.Lines.Select(x => new CreateSupplierQuoteLineDto(
                                    x.ProductId,
                                    x.Quantity
                                ))
                                .ToList()
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/suppliers/quotes/{result.Value}",
                            new { supplierQuoteId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.SupplierQuotesCreate);

        group
            .MapPost(
                "/{id:guid}/respond",
                async (
                    Guid id,
                    RespondSupplierQuoteRequest req,
                    ICommandHandler<RespondSupplierQuoteCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RespondSupplierQuoteCommand(
                            id,
                            req.RespondedAtUtc,
                            req.SupplierMessage,
                            req.Lines.Select(
                                    x => new Domain.SupplierQuotes.SupplierQuoteResponseLineDto(
                                        x.ProductId,
                                        x.UnitPrice,
                                        x.DiscountPerc,
                                        x.TaxPerc,
                                        x.Conditions
                                    )
                                )
                                .ToList()
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.SupplierQuotesRespond);

        group
            .MapPost(
                "/{id:guid}/reject",
                async (
                    Guid id,
                    RejectSupplierQuoteRequest req,
                    ICommandHandler<RejectSupplierQuoteCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RejectSupplierQuoteCommand(id, req.Reason, req.RejectedAtUtc),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.SupplierQuotesReject);
    }
}
