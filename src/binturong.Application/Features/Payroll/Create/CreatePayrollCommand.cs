using Application.Abstractions.Messaging;

namespace Application.Features.Payroll.Create;

public sealed record CreatePayrollCommand(
    string PeriodCode,
    DateOnly StartDate,
    DateOnly EndDate,
    string PayrollType
) : ICommand<Guid>;
