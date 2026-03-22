namespace Application.ReadModels.Accounting;

public sealed class AccountingReconciliationReadModel
{
    public string Id { get; init; } = default!; // "accounting_reconciliation:{Id}"
    public Guid ReconciliationId { get; init; }

    public Guid AccountingEntryId { get; init; }
    public string? AccountingEntryDetail { get; init; }

    public string SourceType { get; init; } = default!;
    public Guid? SourceId { get; init; }

    public decimal MatchedAmount { get; init; }
    public DateTime ReconciledAtUtc { get; init; }
}
