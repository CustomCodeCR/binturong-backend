using SharedKernel;

namespace Domain.Clients;

public sealed record ClientCreatedDomainEvent(
    Guid ClientId,
    string PersonType,
    string IdentificationType,
    string Identification,
    string TradeName,
    string ContactName,
    string Email,
    string PrimaryPhone,
    string? SecondaryPhone,
    string? Industry,
    string? ClientType,
    int Score,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record ClientUpdatedDomainEvent(
    Guid ClientId,
    string TradeName,
    string ContactName,
    string Email,
    string PrimaryPhone,
    string? SecondaryPhone,
    string? Industry,
    string? ClientType,
    int Score,
    bool IsActive,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record ClientDeletedDomainEvent(Guid ClientId) : IDomainEvent;
