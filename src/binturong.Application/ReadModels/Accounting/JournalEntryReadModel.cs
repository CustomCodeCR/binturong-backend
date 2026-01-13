namespace Application.ReadModels.Accounting;

public sealed class JournalEntryReadModel
{
    public string Id { get; init; } = default!; // "je:{JournalEntryId}"
    public int JournalEntryId { get; init; }

    public string EntryType { get; init; } = default!;
    public string Number { get; init; } = default!;
    public DateTime EntryDate { get; init; }

    public int PeriodId { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }

    public string? Description { get; init; }
    public string? SourceModule { get; init; }
    public int? SourceId { get; init; }

    public IReadOnlyList<JournalEntryLineReadModel> Lines { get; init; } = [];
}

public sealed class JournalEntryLineReadModel
{
    public int JournalEntryDetailId { get; init; }

    public int AccountId { get; init; }
    public string AccountCode { get; init; } = default!;
    public string AccountName { get; init; } = default!;

    public decimal Debit { get; init; }
    public decimal Credit { get; init; }

    public int? CostCenterId { get; init; }
    public string? CostCenterCode { get; init; }

    public string? Description { get; init; }
}
