using Application.Abstractions.Messaging;

namespace Application.Features.DebitNotes.Delete;

public sealed record DeleteDebitNoteCommand(Guid DebitNoteId) : ICommand;
