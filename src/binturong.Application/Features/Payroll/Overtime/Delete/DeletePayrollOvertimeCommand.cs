using Application.Abstractions.Messaging;

namespace Application.Features.Payroll.Overtime.Delete;

public sealed record DeletePayrollOvertimeCommand(Guid OvertimeId) : ICommand;
