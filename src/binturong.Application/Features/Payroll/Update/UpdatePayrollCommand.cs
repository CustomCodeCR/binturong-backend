using Application.Abstractions.Messaging;

namespace Application.Features.Payroll.Update;

public sealed record UpdatePayrollCommand(
    Guid PayrollId,
    string PeriodCode,
    DateOnly StartDate,
    DateOnly EndDate,
    string PayrollType,
    string Status
) : ICommand;
