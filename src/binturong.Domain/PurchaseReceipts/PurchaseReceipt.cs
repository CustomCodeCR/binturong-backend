using Domain.PurchaseReceiptDetails;
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

    public ICollection<PurchaseReceiptDetail> Details { get; set; } =
        new List<PurchaseReceiptDetail>();

    // =========================
    // Domain events
    // =========================

    public void RaiseRegistered() =>
        Raise(
            new PurchaseReceiptRegisteredDomainEvent(
                Id,
                PurchaseOrderId,
                WarehouseId,
                ReceiptDate,
                Status,
                string.IsNullOrWhiteSpace(Notes) ? null : Notes
            )
        );

    public void AddDetail(PurchaseReceiptDetail d)
    {
        Details.Add(d);

        Raise(
            new PurchaseReceiptDetailAddedDomainEvent(
                Id,
                d.Id,
                d.ProductId,
                d.QuantityReceived,
                d.UnitCost
            )
        );
    }

    public void Reject(string reason)
    {
        Status = "Rejected";
        Notes = reason;

        Raise(new PurchaseReceiptRejectedDomainEvent(Id, PurchaseOrderId, reason, DateTime.UtcNow));
    }
}
