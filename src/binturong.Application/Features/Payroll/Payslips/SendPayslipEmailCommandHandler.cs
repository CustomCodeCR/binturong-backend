using System.Text;
using Application.Abstractions.Background;
using Application.Abstractions.Data;
using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Payroll.Payslips;

internal sealed class SendPayslipEmailCommandHandler : ICommandHandler<SendPayslipEmailCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IPdfGenerator _pdf;
    private readonly IEmailSender _email;
    private readonly IBackgroundJobScheduler _jobs;

    public SendPayslipEmailCommandHandler(
        IApplicationDbContext db,
        IPdfGenerator pdf,
        IEmailSender email,
        IBackgroundJobScheduler jobs
    )
    {
        _db = db;
        _pdf = pdf;
        _email = email;
        _jobs = jobs;
    }

    public async Task<Result> Handle(SendPayslipEmailCommand cmd, CancellationToken ct)
    {
        var payroll = await _db.Payrolls.FirstOrDefaultAsync(x => x.Id == cmd.PayrollId, ct);
        if (payroll is null)
            return Result.Failure(PayrollErrors.NotFound(cmd.PayrollId));

        if (payroll.Status != "Calculated" && payroll.Status != "Closed")
            return Result.Failure(PayrollErrors.PayrollNotCalculated);

        var detail = await _db
            .PayrollDetails.Include(x => x.Employee)
            .FirstOrDefaultAsync(
                x => x.PayrollId == cmd.PayrollId && x.EmployeeId == cmd.EmployeeId,
                ct
            );

        if (detail is null)
            return Result.Failure(PayrollErrors.DetailNotFound(cmd.EmployeeId));

        var emp = detail.Employee;
        if (emp is null)
            return Result.Failure(PayrollErrors.EmployeeNotFound);

        if (string.IsNullOrWhiteSpace(emp.Email))
            return Result.Failure(PayrollErrors.EmployeeEmailMissing);

        var html = BuildEmailHtml(
            payroll.PeriodCode,
            payroll.StartDate,
            payroll.EndDate,
            emp.FullName,
            detail.NetSalary
        );

        await _jobs.EnqueueAsync(
            async (sp, token) =>
            {
                var pdfBytes = await _pdf.RenderHtmlToPdfAsync(
                    BuildPayslipHtml(
                        payroll.PeriodCode,
                        payroll.StartDate,
                        payroll.EndDate,
                        emp.FullName,
                        emp.NationalId,
                        emp.JobTitle,
                        detail
                    ),
                    token
                );

                var body =
                    html
                    + $"<div style='margin-top:12px;'>Adjunto: boleta de pago (PDF) ({pdfBytes.Length} bytes).</div>";

                await _email.SendAsync(
                    emp.Email.Trim(),
                    $"Boleta de pago {payroll.PeriodCode}",
                    body,
                    token
                );
            },
            ct
        );

        return Result.Success();
    }

    private static string BuildEmailHtml(
        string periodCode,
        DateOnly start,
        DateOnly end,
        string employee,
        decimal net
    )
    {
        var sb = new StringBuilder();
        sb.Append("<html><body>");
        sb.Append($"<div>Hola {Escape(employee)},</div>");
        sb.Append(
            $"<div>Tu boleta de pago del período <b>{Escape(periodCode)}</b> ({start:yyyy-MM-dd} - {end:yyyy-MM-dd}) está lista.</div>"
        );
        sb.Append($"<div>Salario neto: <b>{net:N2}</b></div>");
        sb.Append("<div style='margin-top:10px;'>Saludos.</div>");
        sb.Append("</body></html>");
        return sb.ToString();
    }

    private static string BuildPayslipHtml(
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
