using Application.Abstractions.Messaging;

namespace Application.Features.DebitNotes.Create;

public sealed record CreateDebitNoteCommand(
    Guid InvoiceId,
    DateTime IssueDate,
    string Reason,
    decimal TotalAmount
) : ICommand<Guid>;
