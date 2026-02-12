using Application.Abstractions.Messaging;

namespace Application.Features.Payroll.Payslips;

public sealed record SendPayslipEmailCommand(Guid PayrollId, Guid EmployeeId) : ICommand;
