using SharedKernel;

namespace Domain.Payrolls;

public sealed record PayrollCreatedDomainEvent(
    Guid PayrollId,
    string PeriodCode,
    DateOnly StartDate,
    DateOnly EndDate,
    string PayrollType,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
) : IDomainEvent;

public sealed record PayrollCalculatedDomainEvent(
    Guid PayrollId,
    string Status,
    DateTime UpdatedAtUtc
) : IDomainEvent;

public sealed record PayrollClosedDomainEvent(Guid PayrollId, string Status, DateTime UpdatedAtUtc)
    : IDomainEvent;
