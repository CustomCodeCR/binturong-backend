namespace Application.ReadModels.Sales;

public sealed class DebitNoteReadModel
{
    public string Id { get; init; } = default!; // "debit_note:{DebitNoteId}"
    public int DebitNoteId { get; init; }

    public int InvoiceId { get; init; }
    public string? InvoiceConsecutive { get; init; }

    public string? TaxKey { get; init; }
    public string? Consecutive { get; init; }
    public DateTime IssueDate { get; init; }

    public string Reason { get; init; } = default!;
    public decimal TotalAmount { get; init; }
    public string TaxStatus { get; init; } = default!;
}
