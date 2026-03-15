namespace Application.ReadModels.Inventory;

public sealed class InventoryTransferReadModel
{
    public string Id { get; init; } = default!; // "transfer:{TransferId}"
    public Guid TransferId { get; init; }

    public Guid FromBranchId { get; init; }
    public Guid ToBranchId { get; init; }

    public string? FromBranchCode { get; init; }
    public string? FromBranchName { get; init; }

    public string? ToBranchCode { get; init; }
    public string? ToBranchName { get; init; }

    public string Status { get; init; } = default!;
    public string? Notes { get; init; }

    public Guid CreatedByUserId { get; init; }
    public string? CreatedByUsername { get; init; }
    public string? CreatedByEmail { get; init; }

    public Guid? ApprovedByUserId { get; init; }
    public string? ApprovedByUsername { get; init; }
    public string? ApprovedByEmail { get; init; }

    public string? RejectionReason { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public IReadOnlyList<InventoryTransferLineReadModel> Lines { get; init; } = [];
}

public sealed class InventoryTransferLineReadModel
{
    public Guid LineId { get; init; }

    public Guid ProductId { get; init; }
    public string? ProductSku { get; init; }
    public string? ProductName { get; init; }

    public decimal Quantity { get; init; }

    public Guid FromWarehouseId { get; init; }
    public string? FromWarehouseCode { get; init; }
    public string? FromWarehouseName { get; init; }

    public Guid ToWarehouseId { get; init; }
    public string? ToWarehouseCode { get; init; }
    public string? ToWarehouseName { get; init; }
}
