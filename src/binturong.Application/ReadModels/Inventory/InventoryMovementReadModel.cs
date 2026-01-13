namespace Application.ReadModels.Inventory;

public sealed class InventoryMovementReadModel
{
    public string Id { get; init; } = default!; // "inv_mov:{MovementId}"
    public int MovementId { get; init; }

    public int ProductId { get; init; }
    public string ProductName { get; init; } = default!;

    public int MovementTypeId { get; init; }
    public string MovementTypeCode { get; init; } = default!;
    public string MovementTypeDescription { get; init; } = default!;
    public int Sign { get; init; } // +1 / -1

    public int? WarehouseFromId { get; init; }
    public string? WarehouseFromName { get; init; }

    public int? WarehouseToId { get; init; }
    public string? WarehouseToName { get; init; }

    public DateTime MovementDate { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitCost { get; init; }

    public string? SourceModule { get; init; }
    public int? SourceId { get; init; }
    public string? Notes { get; init; }
}
