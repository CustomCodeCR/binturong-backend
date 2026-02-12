using Application.Abstractions.Messaging;

namespace Application.Features.Payroll.Delete;

public sealed record DeletePayrollCommand(Guid PayrollId) : ICommand;
