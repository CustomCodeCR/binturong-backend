namespace Api.Endpoints.Inventory;

public sealed record RegisterPurchaseEntryRequest(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    decimal UnitCost,
    string? Notes,
    int? SourceId = null
);

public sealed record RegisterServiceExitRequest(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    decimal UnitCost,
    string? Notes,
    int? SourceId = null
);

public sealed record RegisterPhysicalCountAdjustmentRequest(
    Guid ProductId,
    Guid WarehouseId,
    decimal CountedStock,
    decimal UnitCost,
    string Justification
);
