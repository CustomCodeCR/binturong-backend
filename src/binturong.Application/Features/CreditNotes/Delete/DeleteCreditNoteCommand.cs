using Application.Abstractions.Messaging;

namespace Application.Features.CreditNotes.Delete;

public sealed record DeleteCreditNoteCommand(Guid CreditNoteId) : ICommand;
