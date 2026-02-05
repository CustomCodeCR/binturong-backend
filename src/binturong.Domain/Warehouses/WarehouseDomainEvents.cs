using SharedKernel;

namespace Domain.Warehouses;

public sealed record WarehouseCreatedDomainEvent(
    Guid WarehouseId,
    Guid BranchId,
    string BranchCode,
    string BranchName,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record WarehouseUpdatedDomainEvent(
    Guid WarehouseId,
    Guid BranchId,
    string BranchCode,
    string BranchName,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record WarehouseDeletedDomainEvent(Guid WarehouseId, Guid BranchId) : IDomainEvent;
