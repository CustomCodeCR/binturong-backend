using SharedKernel;

namespace Domain.ClientAddresses;

public sealed record ClientAddressCreatedDomainEvent(
    Guid ClientId,
    Guid AddressId,
    string AddressType,
    string AddressLine,
    string Province,
    string Canton,
    string District,
    string? Notes,
    bool IsPrimary,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record ClientAddressUpdatedDomainEvent(
    Guid ClientId,
    Guid AddressId,
    string AddressType,
    string AddressLine,
    string Province,
    string Canton,
    string District,
    string? Notes,
    bool IsPrimary,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record ClientAddressDeletedDomainEvent(Guid ClientId, Guid AddressId) : IDomainEvent;

// Recomendado para garantizar 1 sola primary address por Client
public sealed record ClientPrimaryAddressSetDomainEvent(
    Guid ClientId,
    Guid AddressId,
    DateTime UpdatedAt
) : IDomainEvent;
