using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Application.Features.Reports.GetFinancialReport;
using SharedKernel;

namespace Application.Features.Reports.ExportFinancialReportPdf;

internal sealed class ExportFinancialReportPdfQueryHandler
    : IQueryHandler<ExportFinancialReportPdfQuery, ExportFinancialReportPdfResponse>
{
    private readonly IQueryHandler<
        GetFinancialReportQuery,
        Application.ReadModels.Reports.FinancialReportReadModel
    > _handler;
    private readonly IPdfGenerator _pdf;

    public ExportFinancialReportPdfQueryHandler(
        IQueryHandler<
            GetFinancialReportQuery,
            Application.ReadModels.Reports.FinancialReportReadModel
        > handler,
        IPdfGenerator pdf
    )
    {
        _handler = handler;
        _pdf = pdf;
    }

    public async Task<Result<ExportFinancialReportPdfResponse>> Handle(
        ExportFinancialReportPdfQuery q,
        CancellationToken ct
    )
    {
        var result = await _handler.Handle(new GetFinancialReportQuery(q.FromUtc, q.ToUtc), ct);
        if (result.IsFailure)
            return Result.Failure<ExportFinancialReportPdfResponse>(result.Error);

        var report = result.Value;

        var html = $"""
            <html>
            <body>
                <h1>Financial Report</h1>
                <p>From: {report.FromUtc:yyyy-MM-dd HH:mm:ss} UTC</p>
                <p>To: {report.ToUtc:yyyy-MM-dd HH:mm:ss} UTC</p>
                <table border="1" cellspacing="0" cellpadding="6">
                    <tr><th>Sales</th><td>{report.SalesTotal:N2}</td></tr>
                    <tr><th>Expenses</th><td>{report.ExpensesTotal:N2}</td></tr>
                    <tr><th>Profit</th><td>{report.Profit:N2}</td></tr>
                </table>
                {(report.HasData ? "" : "<p>Sin información disponible</p>")}
            </body>
            </html>
            """;

        var bytes = await _pdf.RenderHtmlToPdfAsync(html, ct);

        return Result.Success(
            new ExportFinancialReportPdfResponse("financial-report.pdf", "application/pdf", bytes)
        );
    }
}
