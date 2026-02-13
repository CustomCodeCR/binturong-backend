namespace Api.Endpoints.DebitNotes;

public sealed record CreateDebitNoteRequest(
    Guid InvoiceId,
    string Reason,
    decimal TotalAmount,
    DateTime IssueDate
);

public sealed record EmitDebitNoteRequest(
    string Mode // "Normal" | "Contingency"
);
