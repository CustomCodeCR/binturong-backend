using SharedKernel;

namespace Domain.SupplierContacts;

public sealed record SupplierContactCreatedDomainEvent(
    Guid SupplierId,
    Guid ContactId,
    string Name,
    string? JobTitle,
    string Email,
    string? Phone,
    bool IsPrimary,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record SupplierContactUpdatedDomainEvent(
    Guid SupplierId,
    Guid ContactId,
    string Name,
    string? JobTitle,
    string Email,
    string? Phone,
    bool IsPrimary,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record SupplierContactDeletedDomainEvent(Guid SupplierId, Guid ContactId)
    : IDomainEvent;

public sealed record SupplierPrimaryContactSetDomainEvent(
    Guid SupplierId,
    Guid ContactId,
    DateTime UpdatedAt
) : IDomainEvent;
