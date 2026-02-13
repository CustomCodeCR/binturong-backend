using Application.Abstractions.Messaging;

namespace Application.Features.Payments.RegisterCard;

public sealed record RegisterCardPaymentCommand(
    Guid InvoiceId,
    Guid ClientId,
    Guid PaymentMethodId,
    DateTime PaymentDate,
    decimal Amount,
    bool IsPosAvailable,
    bool IsApproved,
    string? PosAuthCode,
    string? Notes
) : ICommand<RegisterCardPaymentResponse>;

public sealed record RegisterCardPaymentResponse(
    Guid PaymentId,
    Guid InvoiceId,
    decimal AppliedAmount,
    string InvoiceInternalStatus
);
