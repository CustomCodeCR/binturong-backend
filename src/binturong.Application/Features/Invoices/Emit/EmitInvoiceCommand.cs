using Application.Abstractions.Messaging;

namespace Application.Features.Invoices.Emit;

public sealed record EmitInvoiceCommand(Guid InvoiceId, string Mode)
    : ICommand<EmitInvoiceResponse>;

public sealed record EmitInvoiceResponse(
    string Mode,
    string TaxStatus,
    string TaxKey,
    string Consecutive,
    string PdfKey,
    string XmlKey,
    string? Message
);
