namespace Application.ReadModels.Payables;

public sealed class AccountsPayableReadModel
{
    public string Id { get; init; } = default!; // "ap:{AccountPayableId}"
    public int AccountPayableId { get; init; }

    public int SupplierId { get; init; }
    public string SupplierName { get; init; } = default!;

    public int? PurchaseOrderId { get; init; }
    public string? SupplierInvoiceId { get; init; }

    public DateTime DocumentDate { get; init; }
    public DateTime DueDate { get; init; }

    public decimal TotalAmount { get; init; }
    public decimal PendingBalance { get; init; }

    public string Currency { get; init; } = default!;
    public string Status { get; init; } = default!;
}
