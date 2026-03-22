using SharedKernel;

namespace Domain.Accounting;

public sealed record AccountingEntryCreatedDomainEvent(
    Guid AccountingEntryId,
    string EntryType, // Income | Expense
    decimal Amount,
    string Detail,
    string Category,
    DateTime EntryDateUtc,
    Guid? ClientId,
    Guid? SupplierId,
    string? InvoiceNumber,
    string? ReceiptFileS3Key,
    bool IsReconciled
) : IDomainEvent;

public sealed record AccountingEntryUpdatedDomainEvent(
    Guid AccountingEntryId,
    decimal Amount,
    string Detail,
    string Category,
    DateTime EntryDateUtc,
    Guid? ClientId,
    Guid? SupplierId,
    string? InvoiceNumber,
    string? ReceiptFileS3Key,
    bool IsReconciled,
    DateTime UpdatedAtUtc
) : IDomainEvent;

public sealed record AccountingEntryDeletedDomainEvent(Guid AccountingEntryId) : IDomainEvent;

public sealed record AccountingEntryReconciledDomainEvent(
    Guid AccountingEntryId,
    Guid ReconciliationId,
    decimal MatchedAmount,
    DateTime ReconciledAtUtc
) : IDomainEvent;

public sealed record AccountingEntryUnreconciledDomainEvent(
    Guid AccountingEntryId,
    DateTime UnreconciledAtUtc
) : IDomainEvent;

public sealed record AccountingReconciliationCreatedDomainEvent(
    Guid ReconciliationId,
    Guid AccountingEntryId,
    string SourceType, // Invoice | PurchaseOrder | Manual
    Guid? SourceId,
    decimal MatchedAmount,
    DateTime ReconciledAtUtc
) : IDomainEvent;

public sealed record AccountingReconciliationDeletedDomainEvent(Guid ReconciliationId)
    : IDomainEvent;
