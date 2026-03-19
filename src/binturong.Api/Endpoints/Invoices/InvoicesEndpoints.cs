using Api.Security;
using Application.Abstractions.Messaging;
using Application.Common.Selects;
using Application.Features.Invoices.ConvertFromQuote;
using Application.Features.Invoices.Create;
using Application.Features.Invoices.Delete;
using Application.Features.Invoices.Emit;
using Application.Features.Invoices.GetInvoiceById;
using Application.Features.Invoices.GetInvoices;
using Application.Features.Invoices.GetInvoicesSelect;
using Application.Features.Invoices.Update;
using Application.ReadModels.Sales;
using Application.Security.Scopes;

namespace Api.Endpoints.Invoices;

public sealed class InvoicesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices").WithTags("Invoices");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<GetInvoicesQuery, IReadOnlyList<InvoiceReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetInvoicesQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.InvoicesRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetInvoiceByIdQuery, InvoiceReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetInvoiceByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.InvoicesRead);

        group
            .MapPost(
                "/",
                async (
                    CreateInvoiceRequest req,
                    ICommandHandler<CreateInvoiceCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var featureLines = req
                        .Lines.Select(l =>
                        {
                            var baseAmount = l.Quantity * l.UnitPrice;
                            var discountAmount = Math.Round(
                                baseAmount * (l.DiscountPerc / 100m),
                                2
                            );
                            var taxable = baseAmount - discountAmount;
                            var taxAmount = Math.Round(taxable * (l.TaxPerc / 100m), 2);
                            var computedLineTotal = Math.Round(taxable + taxAmount, 2);

                            return new CreateInvoiceLine(
                                l.ProductId,
                                l.Description,
                                l.Quantity,
                                l.UnitPrice,
                                l.DiscountPerc,
                                l.TaxPerc,
                                computedLineTotal
                            );
                        })
                        .ToList();

                    var subtotal = Math.Round(req.Lines.Sum(x => x.Quantity * x.UnitPrice), 2);

                    var discounts = Math.Round(
                        req.Lines.Sum(x => (x.Quantity * x.UnitPrice) * (x.DiscountPerc / 100m)),
                        2
                    );

                    var taxes = Math.Round(
                        req.Lines.Sum(x =>
                        {
                            var baseAmount = x.Quantity * x.UnitPrice;
                            var discountAmount = baseAmount * (x.DiscountPerc / 100m);
                            var taxable = baseAmount - discountAmount;
                            return taxable * (x.TaxPerc / 100m);
                        }),
                        2
                    );

                    var total = Math.Round((subtotal - discounts) + taxes, 2);

                    var result = await handler.Handle(
                        new CreateInvoiceCommand(
                            req.ClientId,
                            req.BranchId,
                            req.SalesOrderId,
                            req.ContractId,
                            req.IssueDate,
                            req.DocumentType,
                            req.Currency,
                            req.ExchangeRate,
                            subtotal,
                            taxes,
                            discounts,
                            total,
                            req.Notes,
                            featureLines
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/invoices/{result.Value}",
                            new { invoiceId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.InvoicesCreate);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    UpdateInvoiceRequest req,
                    ICommandHandler<UpdateInvoiceFromApiCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new UpdateInvoiceFromApiCommand(
                            id,
                            req.IssueDate,
                            req.DocumentType,
                            req.Currency,
                            req.ExchangeRate,
                            req.Notes,
                            req.InternalStatus
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.InvoicesUpdate);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    ICommandHandler<DeleteInvoiceCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new DeleteInvoiceCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.InvoicesDelete);

        group
            .MapPost(
                "/{id:guid}/emit",
                async (
                    Guid id,
                    EmitInvoiceRequest req,
                    ICommandHandler<EmitInvoiceCommand, EmitInvoiceResponse> handler,
                    CancellationToken ct
                ) =>
                {
                    var mode = string.IsNullOrWhiteSpace(req.Mode) ? "Normal" : req.Mode.Trim();
                    var result = await handler.Handle(new EmitInvoiceCommand(id, mode), ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.InvoicesEmit);

        group
            .MapPost(
                "/convert-from-quote/{quoteId:guid}",
                async (
                    Guid quoteId,
                    ConvertQuoteToInvoiceRequest req,
                    ICommandHandler<ConvertQuoteToInvoiceCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var mode = string.IsNullOrWhiteSpace(req.Mode) ? "Normal" : req.Mode.Trim();

                    var result = await handler.Handle(
                        new ConvertQuoteToInvoiceCommand(
                            quoteId,
                            req.BranchId,
                            req.IssueDate,
                            req.DocumentType,
                            mode
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/invoices/{result.Value}",
                            new { invoiceId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.InvoicesConvertFromQuote);

        group
            .MapGet(
                "/select",
                async (
                    string? search,
                    IQueryHandler<GetInvoicesSelectQuery, IReadOnlyList<SelectOptionDto>> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetInvoicesSelectQuery(search), ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.InvoicesRead);
    }
}
