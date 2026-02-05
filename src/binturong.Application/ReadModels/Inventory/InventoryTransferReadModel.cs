namespace Application.ReadModels.Inventory;

public sealed class InventoryTransferReadModel
{
    public string Id { get; init; } = default!; // "transfer:{TransferId}"
    public Guid TransferId { get; init; }

    public Guid FromBranchId { get; init; }
    public Guid ToBranchId { get; init; }

    public string Status { get; init; } = default!;
    public string? Notes { get; init; }

    public Guid CreatedByUserId { get; init; }
    public Guid? ApprovedByUserId { get; init; }
    public string? RejectionReason { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public IReadOnlyList<InventoryTransferLineReadModel> Lines { get; init; } = [];
}

public sealed class InventoryTransferLineReadModel
{
    public Guid LineId { get; init; }
    public Guid ProductId { get; init; }
    public decimal Quantity { get; init; }
    public Guid FromWarehouseId { get; init; }
    public Guid ToWarehouseId { get; init; }
}
