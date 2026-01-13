namespace Application.ReadModels.Payroll;

public sealed class PayrollReadModel
{
    public string Id { get; init; } = default!; // "payroll:{PayrollId}"
    public int PayrollId { get; init; }

    public string PeriodCode { get; init; } = default!;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    public string PayrollType { get; init; } = default!;
    public string Status { get; init; } = default!;

    public IReadOnlyList<PayrollDetailReadModel> Details { get; init; } = [];
}

public sealed class PayrollDetailReadModel
{
    public int PayrollDetailId { get; init; }
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = default!;

    public decimal GrossSalary { get; init; }
    public decimal OvertimeHours { get; init; }
    public decimal Deductions { get; init; }
    public decimal EmployerContrib { get; init; }
    public decimal NetSalary { get; init; }
}
