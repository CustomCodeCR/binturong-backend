using SharedKernel;

namespace Domain.ProductCategories;

public sealed record ProductCategoryCreatedDomainEvent(
    Guid CategoryId,
    string Name,
    string? Description,
    bool IsActive
) : IDomainEvent;

public sealed record ProductCategoryUpdatedDomainEvent(
    Guid CategoryId,
    string Name,
    string? Description,
    bool IsActive
) : IDomainEvent;

public sealed record ProductCategoryDeletedDomainEvent(Guid CategoryId) : IDomainEvent;
