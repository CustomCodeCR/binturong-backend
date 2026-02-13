namespace Application.ReadModels.Sales;

public sealed class DebitNoteReadModel
{
    public string Id { get; init; } = default!; // "debit_note:{DebitNoteId}"
    public Guid DebitNoteId { get; init; }

    public Guid InvoiceId { get; init; }
    public string? InvoiceConsecutive { get; init; }

    public string? TaxKey { get; init; }
    public string? Consecutive { get; init; }
    public DateTime IssueDate { get; init; }

    public string Reason { get; init; } = default!;
    public decimal TotalAmount { get; init; }
    public string TaxStatus { get; init; } = default!;

    // Storage keys (S3/MinIO/etc.)
    public string? PdfS3Key { get; init; }
    public string? XmlS3Key { get; init; }
}
