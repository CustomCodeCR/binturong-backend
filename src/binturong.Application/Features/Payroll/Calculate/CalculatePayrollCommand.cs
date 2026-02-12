using Application.Abstractions.Messaging;

namespace Application.Features.Payroll.Calculate;

public sealed record CalculatePayrollCommand(
    string PeriodCode,
    DateOnly StartDate,
    DateOnly EndDate,
    string PayrollType,
    bool AttendanceConfirmed
) : ICommand<Guid>;
