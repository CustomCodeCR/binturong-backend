namespace Api.Endpoints.CreditNotes;

public sealed record CreateCreditNoteRequest(
    Guid InvoiceId,
    string Reason,
    decimal TotalAmount,
    DateTime IssueDate
);

public sealed record EmitCreditNoteRequest(
    string Mode // "Normal" | "Contingency"
);
