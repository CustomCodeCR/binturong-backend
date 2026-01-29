using SharedKernel;

namespace Domain.UnitsOfMeasure;

public sealed record UnitOfMeasureCreatedDomainEvent(
    Guid UomId,
    string Code,
    string Name,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record UnitOfMeasureUpdatedDomainEvent(
    Guid UomId,
    string Code,
    string Name,
    bool IsActive,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record UnitOfMeasureDeletedDomainEvent(Guid UomId) : IDomainEvent;
