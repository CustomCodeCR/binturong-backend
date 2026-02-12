using System.Text;
using Application.Abstractions.Data;
using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Payroll.Payslips;

internal sealed class GeneratePayslipPdfQueryHandler
    : IQueryHandler<GeneratePayslipPdfQuery, byte[]>
{
    private readonly IApplicationDbContext _db;
    private readonly IPdfGenerator _pdf;

    public GeneratePayslipPdfQueryHandler(IApplicationDbContext db, IPdfGenerator pdf)
    {
        _db = db;
        _pdf = pdf;
    }

    public async Task<Result<byte[]>> Handle(GeneratePayslipPdfQuery q, CancellationToken ct)
    {
        var payroll = await _db.Payrolls.FirstOrDefaultAsync(x => x.Id == q.PayrollId, ct);
        if (payroll is null)
            return Result.Failure<byte[]>(PayrollErrors.NotFound(q.PayrollId));

        if (payroll.Status != "Calculated" && payroll.Status != "Closed")
            return Result.Failure<byte[]>(PayrollErrors.PayrollNotCalculated);

        var detail = await _db
            .PayrollDetails.Include(x => x.Employee)
            .FirstOrDefaultAsync(
                x => x.PayrollId == q.PayrollId && x.EmployeeId == q.EmployeeId,
                ct
            );

        if (detail is null)
            return Result.Failure<byte[]>(PayrollErrors.DetailNotFound(q.EmployeeId));

        var emp = detail.Employee;
        if (emp is null)
            return Result.Failure<byte[]>(PayrollErrors.EmployeeNotFound);

        var html = BuildHtml(
            payroll.PeriodCode,
            payroll.StartDate,
            payroll.EndDate,
            emp.FullName,
            emp.NationalId,
            emp.JobTitle,
            detail
        );

        var bytes = await _pdf.RenderHtmlToPdfAsync(html, ct);
        return Result.Success(bytes);
    }

    private static string BuildHtml(
        string periodCode,
        DateOnly start,
        DateOnly end,
        string employeeName,
        string nationalId,
        string jobTitle,
        Domain.PayrollDetails.PayrollDetail d
    )
    {
        var sb = new StringBuilder();
        sb.Append("<html><head><meta charset='utf-8'/></head><body>");
        sb.Append("<h2>Boleta de Pago</h2>");
        sb.Append(
            $"<div><b>Periodo:</b> {periodCode} ({start:yyyy-MM-dd} - {end:yyyy-MM-dd})</div>"
        );
        sb.Append($"<div><b>Empleado:</b> {Escape(employeeName)}</div>");
        sb.Append($"<div><b>Cédula:</b> {Escape(nationalId)}</div>");
        sb.Append($"<div><b>Puesto:</b> {Escape(jobTitle)}</div>");
        sb.Append("<hr/>");
        sb.Append(
            "<table style='width:100%; border-collapse:collapse;' border='1' cellspacing='0' cellpadding='6'>"
        );
        sb.Append("<tr><th align='left'>Concepto</th><th align='right'>Monto</th></tr>");
        sb.Append($"<tr><td>Salario Bruto</td><td align='right'>{d.GrossSalary:N2}</td></tr>");
        sb.Append($"<tr><td>Horas Extra</td><td align='right'>{d.OvertimeHours:N2}</td></tr>");
        sb.Append($"<tr><td>Comisiones</td><td align='right'>{d.CommissionAmount:N2}</td></tr>");
        sb.Append($"<tr><td>Deducciones</td><td align='right'>{d.Deductions:N2}</td></tr>");
        sb.Append(
            $"<tr><td>Aporte Patronal</td><td align='right'>{d.EmployerContrib:N2}</td></tr>"
        );
        sb.Append(
            $"<tr><td><b>Salario Neto</b></td><td align='right'><b>{d.NetSalary:N2}</b></td></tr>"
        );
        sb.Append("</table>");
        sb.Append("</body></html>");
        return sb.ToString();
    }

    private static string Escape(string s) =>
        (s ?? string.Empty).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
