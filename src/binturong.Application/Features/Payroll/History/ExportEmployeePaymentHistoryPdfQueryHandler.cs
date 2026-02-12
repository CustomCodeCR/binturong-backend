using System.Text;
using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Features.Payroll.History;

internal sealed class ExportEmployeePaymentHistoryPdfQueryHandler
    : IQueryHandler<ExportEmployeePaymentHistoryPdfQuery, byte[]>
{
    private readonly IQueryHandler<
        GetEmployeePaymentHistoryQuery,
        IReadOnlyList<EmployeePaymentHistoryRow>
    > _history;
    private readonly IPdfGenerator _pdf;

    public ExportEmployeePaymentHistoryPdfQueryHandler(
        IQueryHandler<
            GetEmployeePaymentHistoryQuery,
            IReadOnlyList<EmployeePaymentHistoryRow>
        > history,
        IPdfGenerator pdf
    )
    {
        _history = history;
        _pdf = pdf;
    }

    public async Task<Result<byte[]>> Handle(
        ExportEmployeePaymentHistoryPdfQuery q,
        CancellationToken ct
    )
    {
        var res = await _history.Handle(
            new GetEmployeePaymentHistoryQuery(q.EmployeeId, q.FromUtc, q.ToUtc),
            ct
        );
        if (res.IsFailure)
            return Result.Failure<byte[]>(res.Error);

        var html = BuildHtml(res.Value);
        var bytes = await _pdf.RenderHtmlToPdfAsync(html, ct);
        return Result.Success(bytes);
    }

    private static string BuildHtml(IReadOnlyList<EmployeePaymentHistoryRow> rows)
    {
        var sb = new StringBuilder();
        sb.Append("<html><head><meta charset='utf-8'/></head><body>");
        sb.Append("<h2>Historial de pagos</h2>");
        sb.Append(
            "<table style='width:100%; border-collapse:collapse;' border='1' cellspacing='0' cellpadding='6'>"
        );
        sb.Append("<tr>");
        sb.Append(
            "<th>Periodo</th><th>Inicio</th><th>Fin</th><th>Estado</th><th align='right'>Bruto</th><th align='right'>Extra</th><th align='right'>Comisión</th><th align='right'>Deducciones</th><th align='right'>Neto</th>"
        );
        sb.Append("</tr>");

        foreach (var r in rows)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{Escape(r.PeriodCode)}</td>");
            sb.Append($"<td>{r.StartDateUtc:yyyy-MM-dd}</td>");
            sb.Append($"<td>{r.EndDateUtc:yyyy-MM-dd}</td>");
            sb.Append($"<td>{Escape(r.Status)}</td>");
            sb.Append($"<td align='right'>{r.GrossSalary:N2}</td>");
            sb.Append($"<td align='right'>{r.OvertimeHours:N2}</td>");
            sb.Append($"<td align='right'>{r.CommissionAmount:N2}</td>");
            sb.Append($"<td align='right'>{r.Deductions:N2}</td>");
            sb.Append($"<td align='right'>{r.NetSalary:N2}</td>");
            sb.Append("</tr>");
        }

        sb.Append("</table></body></html>");
        return sb.ToString();
    }

    private static string Escape(string s) =>
        (s ?? string.Empty).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
