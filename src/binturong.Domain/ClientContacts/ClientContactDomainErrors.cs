using SharedKernel;

namespace Domain.ClientContacts;

public sealed record ClientContactCreatedDomainEvent(
    Guid ClientId,
    Guid ContactId,
    string Name,
    string? JobTitle,
    string Email,
    string? Phone,
    bool IsPrimary,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record ClientContactUpdatedDomainEvent(
    Guid ClientId,
    Guid ContactId,
    string Name,
    string? JobTitle,
    string Email,
    string? Phone,
    bool IsPrimary,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record ClientContactDeletedDomainEvent(Guid ClientId, Guid ContactId) : IDomainEvent;

public sealed record ClientPrimaryContactSetDomainEvent(
    Guid ClientId,
    Guid ContactId,
    DateTime UpdatedAt
) : IDomainEvent;
