namespace Application.ReadModels.Accounting;

public sealed class AccountingReconciliationSummaryReadModel
{
    public int MatchedCount { get; init; }
    public int UnmatchedCount { get; init; }

    public IReadOnlyList<AccountingUnmatchedItemReadModel> Differences { get; init; } = [];
}

public sealed class AccountingUnmatchedItemReadModel
{
    public Guid AccountingEntryId { get; init; }
    public string EntryType { get; init; } = default!;
    public decimal Amount { get; init; }
    public string Detail { get; init; } = default!;
    public DateTime EntryDateUtc { get; init; }
    public string? InvoiceNumber { get; init; }
}
