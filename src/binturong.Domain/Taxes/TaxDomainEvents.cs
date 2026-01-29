using SharedKernel;

namespace Domain.Taxes;

public sealed record TaxCreatedDomainEvent(
    Guid TaxId,
    string Name,
    string Code,
    decimal Percentage,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record TaxUpdatedDomainEvent(
    Guid TaxId,
    string Name,
    string Code,
    decimal Percentage,
    bool IsActive,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record TaxDeletedDomainEvent(Guid TaxId) : IDomainEvent;
