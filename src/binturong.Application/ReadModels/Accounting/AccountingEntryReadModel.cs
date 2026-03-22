namespace Application.ReadModels.Accounting;

public sealed class AccountingEntryReadModel
{
    public string Id { get; init; } = default!; // "accounting_entry:{Id}"
    public Guid AccountingEntryId { get; init; }

    public string EntryType { get; init; } = default!;
    public decimal Amount { get; init; }
    public string Detail { get; init; } = default!;
    public string Category { get; init; } = default!;

    public DateTime EntryDateUtc { get; init; }

    public Guid? ClientId { get; init; }
    public string? ClientName { get; init; }

    public Guid? SupplierId { get; init; }
    public string? SupplierName { get; init; }

    public string? InvoiceNumber { get; init; }
    public string? ReceiptFileS3Key { get; init; }

    public bool IsReconciled { get; init; }
    public Guid? ReconciliationId { get; init; }
}
