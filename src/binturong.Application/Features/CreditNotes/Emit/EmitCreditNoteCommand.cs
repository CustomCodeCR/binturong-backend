using Application.Abstractions.Messaging;

namespace Application.Features.CreditNotes.Emit;

public sealed record EmitCreditNoteCommand(Guid CreditNoteId) : ICommand<EmitCreditNoteResponse>;

public sealed record EmitCreditNoteResponse(
    string Mode,
    string TaxStatus,
    string TaxKey,
    string Consecutive,
    string PdfKey,
    string XmlKey,
    string? Message
);
