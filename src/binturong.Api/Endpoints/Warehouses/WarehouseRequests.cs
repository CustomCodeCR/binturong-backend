namespace Api.Endpoints.Warehouses;

public sealed record CreateWarehouseRequest(
    Guid BranchId,
    string Code,
    string Name,
    string? Description,
    bool IsActive = true
);

public sealed record UpdateWarehouseRequest(
    string Code,
    string Name,
    string? Description,
    bool IsActive
);
