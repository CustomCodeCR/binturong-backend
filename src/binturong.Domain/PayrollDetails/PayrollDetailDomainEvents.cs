using SharedKernel;

namespace Domain.PayrollDetails;

public sealed record PayrollDetailCalculatedDomainEvent(
    Guid PayrollId,
    Guid PayrollDetailId,
    Guid EmployeeId,
    decimal GrossSalary,
    decimal OvertimeHours,
    decimal CommissionAmount,
    decimal Deductions,
    decimal EmployerContrib,
    decimal NetSalary,
    DateTime UpdatedAtUtc
) : IDomainEvent;

public sealed record PayrollCommissionAdjustedDomainEvent(
    Guid PayrollId,
    Guid PayrollDetailId,
    Guid EmployeeId,
    decimal CommissionAmount,
    DateTime UpdatedAtUtc
) : IDomainEvent;
