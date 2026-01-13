using SharedKernel;

namespace Domain.AccountsPayable;

public sealed class AccountPayable : Entity
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string SupplierInvoiceId { get; set; } = string.Empty;
    public DateOnly DocumentDate { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PendingBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public Domain.Suppliers.Supplier? Supplier { get; set; }
    public Domain.PurchaseOrders.PurchaseOrder? PurchaseOrder { get; set; }
}
