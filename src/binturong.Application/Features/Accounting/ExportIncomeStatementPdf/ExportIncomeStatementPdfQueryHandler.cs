using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Application.Features.Accounting.GetIncomeStatement;
using SharedKernel;

namespace Application.Features.Accounting.ExportIncomeStatementPdf;

internal sealed class ExportIncomeStatementPdfQueryHandler
    : IQueryHandler<ExportIncomeStatementPdfQuery, ExportIncomeStatementPdfResponse>
{
    private readonly IQueryHandler<
        GetIncomeStatementQuery,
        Application.ReadModels.Accounting.IncomeStatementReadModel
    > _handler;
    private readonly IPdfGenerator _pdf;

    public ExportIncomeStatementPdfQueryHandler(
        IQueryHandler<
            GetIncomeStatementQuery,
            Application.ReadModels.Accounting.IncomeStatementReadModel
        > handler,
        IPdfGenerator pdf
    )
    {
        _handler = handler;
        _pdf = pdf;
    }

    public async Task<Result<ExportIncomeStatementPdfResponse>> Handle(
        ExportIncomeStatementPdfQuery q,
        CancellationToken ct
    )
    {
        var result = await _handler.Handle(new GetIncomeStatementQuery(q.FromUtc, q.ToUtc), ct);
        if (result.IsFailure)
            return Result.Failure<ExportIncomeStatementPdfResponse>(result.Error);

        var r = result.Value;

        var html = $"""
            <html>
            <body>
                <h1>Income Statement</h1>
                <p>From: {r.FromUtc:yyyy-MM-dd HH:mm:ss} UTC</p>
                <p>To: {r.ToUtc:yyyy-MM-dd HH:mm:ss} UTC</p>
                <table border="1" cellspacing="0" cellpadding="6">
                    <tr><th>Total Income</th><td>{r.TotalIncome:N2}</td></tr>
                    <tr><th>Total Expenses</th><td>{r.TotalExpenses:N2}</td></tr>
                    <tr><th>Gross Profit</th><td>{r.GrossProfit:N2}</td></tr>
                    <tr><th>Net Profit</th><td>{r.NetProfit:N2}</td></tr>
                </table>
                {(r.HasData ? "" : "<p>No hay movimientos registrados</p>")}
            </body>
            </html>
            """;

        var bytes = await _pdf.RenderHtmlToPdfAsync(html, ct);

        return Result.Success(
            new ExportIncomeStatementPdfResponse("income-statement.pdf", "application/pdf", bytes)
        );
    }
}
