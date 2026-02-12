namespace Application.Features.Payroll.History;

public sealed class EmployeePaymentHistoryRow
{
    public Guid PayrollId { get; init; }
    public string PeriodCode { get; init; } = default!;
    public DateTime StartDateUtc { get; init; }
    public DateTime EndDateUtc { get; init; }
    public string Status { get; init; } = default!;
    public decimal GrossSalary { get; init; }
    public decimal OvertimeHours { get; init; }
    public decimal CommissionAmount { get; init; }
    public decimal Deductions { get; init; }
    public decimal EmployerContrib { get; init; }
    public decimal NetSalary { get; init; }
}
