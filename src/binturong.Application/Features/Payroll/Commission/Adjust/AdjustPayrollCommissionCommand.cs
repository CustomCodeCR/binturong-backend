using Application.Abstractions.Messaging;

namespace Application.Features.Payroll.Commission.Adjust;

public sealed record AdjustPayrollCommissionCommand(
    Guid PayrollId,
    Guid PayrollDetailId,
    decimal CommissionAmount
) : ICommand;
