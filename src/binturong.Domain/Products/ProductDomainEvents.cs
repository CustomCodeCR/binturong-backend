using SharedKernel;

namespace Domain.Products;

public sealed record ProductCreatedDomainEvent(
    Guid ProductId,
    string SKU,
    string? Barcode,
    string Name,
    string? Description,
    Guid? CategoryId,
    Guid? UomId,
    Guid? TaxId,
    decimal BasePrice,
    decimal AverageCost,
    bool IsService,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record ProductUpdatedDomainEvent(
    Guid ProductId,
    string SKU,
    string? Barcode,
    string Name,
    string? Description,
    Guid? CategoryId,
    Guid? UomId,
    Guid? TaxId,
    decimal BasePrice,
    decimal AverageCost,
    bool IsService,
    bool IsActive,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record ProductDeletedDomainEvent(Guid ProductId) : IDomainEvent;
