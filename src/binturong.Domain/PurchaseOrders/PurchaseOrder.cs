using Domain.PurchaseOrderDetails;
using SharedKernel;

namespace Domain.PurchaseOrders;

public sealed class PurchaseOrder : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? RequestId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Taxes { get; set; }
    public decimal Discounts { get; set; }
    public decimal Total { get; set; }

    public Domain.Suppliers.Supplier? Supplier { get; set; }
    public Domain.Branches.Branch? Branch { get; set; }
    public Domain.PurchaseRequests.PurchaseRequest? Request { get; set; }

    public ICollection<PurchaseOrderDetail> Details { get; set; } = new List<PurchaseOrderDetail>();
    public ICollection<Domain.PurchaseReceipts.PurchaseReceipt> Receipts { get; set; } =
        new List<Domain.PurchaseReceipts.PurchaseReceipt>();
    public ICollection<Domain.AccountsPayable.AccountPayable> AccountsPayables { get; set; } =
        new List<Domain.AccountsPayable.AccountPayable>();

    // =========================
    // Domain events
    // =========================

    public void RaiseCreated() =>
        Raise(
            new PurchaseOrderCreatedDomainEvent(
                Id,
                Code,
                SupplierId,
                BranchId,
                RequestId,
                OrderDate,
                Status,
                Currency,
                ExchangeRate,
                Subtotal,
                Taxes,
                Discounts,
                Total
            )
        );

    public void AddDetail(PurchaseOrderDetail d)
    {
        Details.Add(d);

        Raise(
            new PurchaseOrderLineAddedDomainEvent(
                Id,
                d.Id,
                d.ProductId,
                d.Quantity,
                d.UnitPrice,
                d.DiscountPerc,
                d.TaxPerc,
                d.LineTotal
            )
        );
    }

    public void MarkCompleted()
    {
        Status = "Completed";
        Raise(new PurchaseOrderStatusChangedDomainEvent(Id, Status, DateTime.UtcNow, null));
    }

    public void MarkPartiallyReceived()
    {
        Status = "PartiallyReceived";
        Raise(new PurchaseOrderStatusChangedDomainEvent(Id, Status, DateTime.UtcNow, null));
    }

    public void MarkPending(string? reason = null)
    {
        Status = "Pending";
        Raise(new PurchaseOrderStatusChangedDomainEvent(Id, Status, DateTime.UtcNow, reason));
    }
}
