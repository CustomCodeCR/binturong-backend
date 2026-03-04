namespace Application.ReadModels.Payroll;

public sealed class PayrollOvertimeReadModel
{
    public string Id { get; init; } = default!; // "payroll_overtime:{OvertimeId}"
    public Guid OvertimeId { get; init; }

    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;

    public DateTime WorkDateUtc { get; init; }
    public decimal Hours { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}
