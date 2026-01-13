namespace Application.ReadModels.Purchases;

public sealed class PurchaseReceiptReadModel
{
    public string Id { get; init; } = default!; // "purchase_receipt:{ReceiptId}"
    public int ReceiptId { get; init; }

    public int PurchaseOrderId { get; init; }
    public string PurchaseOrderCode { get; init; } = default!;

    public int WarehouseId { get; init; }
    public string WarehouseName { get; init; } = default!;

    public DateTime ReceiptDate { get; init; }
    public string Status { get; init; } = default!;
    public string? Notes { get; init; }

    public IReadOnlyList<PurchaseReceiptLineReadModel> Lines { get; init; } = [];
}

public sealed class PurchaseReceiptLineReadModel
{
    public int ReceiptDetailId { get; init; }

    public int ProductId { get; init; }
    public string ProductName { get; init; } = default!;

    public decimal QuantityReceived { get; init; }
    public decimal UnitCost { get; init; }
}
