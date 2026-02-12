using Application.Abstractions.Messaging;

namespace Application.Features.Payroll.Overtime.Register;

public sealed record RegisterPayrollOvertimeCommand(
    Guid EmployeeId,
    DateOnly WorkDate,
    decimal Hours,
    string? Notes
) : ICommand<Guid>;
