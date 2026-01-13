using SharedKernel;

namespace Domain.PurchaseReceipts;

public sealed class PurchaseReceipt : Entity
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid WarehouseId { get; set; }
    public DateTime ReceiptDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public Domain.PurchaseOrders.PurchaseOrder? PurchaseOrder { get; set; }
    public Domain.Warehouses.Warehouse? Warehouse { get; set; }

    public ICollection<Domain.PurchaseReceiptDetails.PurchaseReceiptDetail> Details { get; set; } =
        new List<Domain.PurchaseReceiptDetails.PurchaseReceiptDetail>();
}
