namespace Application.ReadModels.Purchases;

public sealed class PurchaseReceiptReadModel
{
    public string Id { get; set; } = default!; // "purchase_receipt:{ReceiptId}"
    public Guid ReceiptId { get; set; }

    public Guid PurchaseOrderId { get; set; }
    public string PurchaseOrderCode { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;

    public DateTime ReceiptDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public List<PurchaseReceiptLineReadModel> Lines { get; set; } = new();
}

public sealed class PurchaseReceiptLineReadModel
{
    public Guid ReceiptDetailId { get; set; }

    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;

    public decimal QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }
}
