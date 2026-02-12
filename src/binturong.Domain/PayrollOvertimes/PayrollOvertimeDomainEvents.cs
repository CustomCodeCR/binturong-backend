using SharedKernel;

namespace Domain.PayrollOvertimes;

public sealed record PayrollOvertimeRegisteredDomainEvent(
    Guid OvertimeId,
    Guid EmployeeId,
    DateOnly WorkDate,
    decimal Hours,
    DateTime CreatedAtUtc
) : IDomainEvent;

public sealed record PayrollOvertimeDeletedDomainEvent(Guid OvertimeId, Guid EmployeeId)
    : IDomainEvent;
