using SharedKernel;

namespace Domain.PaymentMethods;

public sealed record PaymentMethodCreatedDomainEvent(
    Guid PaymentMethodId,
    string Code,
    string Description,
    bool IsActive,
    DateTime AtUtc
) : IDomainEvent;

public sealed record PaymentMethodUpdatedDomainEvent(
    Guid PaymentMethodId,
    string Code,
    string Description,
    bool IsActive,
    DateTime AtUtc
) : IDomainEvent;

public sealed record PaymentMethodDeletedDomainEvent(Guid PaymentMethodId, DateTime AtUtc)
    : IDomainEvent;
