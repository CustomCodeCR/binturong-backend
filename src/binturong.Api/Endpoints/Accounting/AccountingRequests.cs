namespace Api.Endpoints.Accounting;

public sealed record CreateIncomeRequest(
    decimal Amount,
    string Detail,
    string Category,
    DateTime EntryDateUtc,
    Guid ClientId,
    string InvoiceNumber
);

public sealed record CreateExpenseRequest(
    decimal Amount,
    string Detail,
    string Category,
    DateTime EntryDateUtc,
    Guid SupplierId,
    string? ReceiptFileS3Key
);
