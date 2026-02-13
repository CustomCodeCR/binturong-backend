using Application.Abstractions.Messaging;

namespace Application.Features.CreditNotes.Create;

public sealed record CreateCreditNoteCommand(
    Guid InvoiceId,
    DateTime IssueDate,
    string Reason,
    decimal TotalAmount
) : ICommand<Guid>;
