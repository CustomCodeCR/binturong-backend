namespace Application.Options;

public sealed class PayrollOptions
{
    public decimal OvertimeHourlyRate { get; init; } = 0m;
    public decimal MaxOvertimeHoursPerEntry { get; init; } = 8m;
    public decimal CommissionPercent { get; init; } = 0m;
    public decimal DeductionsPercent { get; init; } = 0m;
    public decimal EmployerContribPercent { get; init; } = 0m;
}
