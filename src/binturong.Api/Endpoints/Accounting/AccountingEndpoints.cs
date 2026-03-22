using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Accounting.CreateExpense;
using Application.Features.Accounting.CreateIncome;
using Application.Features.Accounting.ExportCashFlowExcel;
using Application.Features.Accounting.ExportIncomeStatementPdf;
using Application.Features.Accounting.GetCashFlow;
using Application.Features.Accounting.GetEntries;
using Application.Features.Accounting.GetIncomeStatement;
using Application.Features.Accounting.GetReconciliationSummary;
using Application.ReadModels.Accounting;
using Application.Security.Scopes;

namespace Api.Endpoints.Accounting;

public sealed class AccountingEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounting").WithTags("Accounting");

        group
            .MapGet(
                "/entries",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    string? entryType,
                    IQueryHandler<
                        GetAccountingEntriesQuery,
                        IReadOnlyList<AccountingEntryReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetAccountingEntriesQuery(page ?? 1, pageSize ?? 50, search, entryType),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.AccountingRead);

        group
            .MapPost(
                "/income",
                async (
                    CreateIncomeRequest req,
                    ICommandHandler<CreateIncomeCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreateIncomeCommand(
                            req.Amount,
                            req.Detail,
                            req.Category,
                            req.EntryDateUtc,
                            req.ClientId,
                            req.InvoiceNumber
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            "/api/accounting/entries",
                            new { accountingEntryId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.AccountingCreateIncome);

        group
            .MapPost(
                "/expense",
                async (
                    CreateExpenseRequest req,
                    ICommandHandler<CreateExpenseCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreateExpenseCommand(
                            req.Amount,
                            req.Detail,
                            req.Category,
                            req.EntryDateUtc,
                            req.SupplierId,
                            req.ReceiptFileS3Key
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            "/api/accounting/entries",
                            new { accountingEntryId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.AccountingCreateExpense);

        group
            .MapGet(
                "/income-statement",
                async (
                    DateTime fromUtc,
                    DateTime toUtc,
                    IQueryHandler<GetIncomeStatementQuery, IncomeStatementReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetIncomeStatementQuery(fromUtc, toUtc),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.AccountingIncomeStatementRead);

        group
            .MapGet(
                "/income-statement/export-pdf",
                async (
                    DateTime fromUtc,
                    DateTime toUtc,
                    IQueryHandler<
                        ExportIncomeStatementPdfQuery,
                        ExportIncomeStatementPdfResponse
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportIncomeStatementPdfQuery(fromUtc, toUtc),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value.Content,
                            result.Value.ContentType,
                            result.Value.FileName
                        );
                }
            )
            .RequireScope(SecurityScopes.AccountingIncomeStatementExport);

        group
            .MapGet(
                "/cash-flow",
                async (
                    DateTime fromUtc,
                    DateTime toUtc,
                    IQueryHandler<GetCashFlowQuery, CashFlowReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetCashFlowQuery(fromUtc, toUtc), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.AccountingCashFlowRead);

        group
            .MapGet(
                "/cash-flow/export-excel",
                async (
                    DateTime fromUtc,
                    DateTime toUtc,
                    IQueryHandler<ExportCashFlowExcelQuery, ExportCashFlowExcelResponse> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportCashFlowExcelQuery(fromUtc, toUtc),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value.Content,
                            result.Value.ContentType,
                            result.Value.FileName
                        );
                }
            )
            .RequireScope(SecurityScopes.AccountingCashFlowExport);

        group
            .MapGet(
                "/reconciliation-summary",
                async (
                    IQueryHandler<
                        GetAccountingReconciliationSummaryQuery,
                        AccountingReconciliationSummaryReadModel
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetAccountingReconciliationSummaryQuery(),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.AccountingReconcile);
    }
}
