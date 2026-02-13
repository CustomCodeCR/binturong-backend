using Application.Abstractions.Messaging;

namespace Application.Features.Payments.RegisterTransfer;

public sealed record RegisterBankTransferPaymentCommand(
    Guid InvoiceId,
    Guid ClientId,
    Guid PaymentMethodId,
    DateTime PaymentDate,
    decimal Amount,
    string Reference, // comprobante
    bool IsBankConfirmed, // true=confirmado, false=pendiente verificación
    string? Notes
) : ICommand<RegisterBankTransferPaymentResponse>;

public sealed record RegisterBankTransferPaymentResponse(
    Guid PaymentId,
    Guid InvoiceId,
    decimal AppliedAmount,
    string InvoiceInternalStatus
);
