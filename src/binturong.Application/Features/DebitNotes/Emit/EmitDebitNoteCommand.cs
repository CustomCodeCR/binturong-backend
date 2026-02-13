using Application.Abstractions.Messaging;

namespace Application.Features.DebitNotes.Emit;

public sealed record EmitDebitNoteCommand(Guid DebitNoteId) : ICommand<EmitDebitNoteResponse>;

public sealed record EmitDebitNoteResponse(
    string Mode,
    string TaxStatus,
    string TaxKey,
    string Consecutive,
    string PdfKey,
    string XmlKey,
    string? Message
);
