using Application.Abstractions.Messaging;

namespace Application.Features.Payments.RegisterCash;

public sealed record RegisterCashPaymentCommand(
    Guid InvoiceId,
    Guid ClientId,
    Guid PaymentMethodId,
    DateTime PaymentDate,
    decimal AmountTendered, // lo que entrega el cliente
    string? Notes
) : ICommand<RegisterCashPaymentResponse>;

public sealed record RegisterCashPaymentResponse(
    Guid PaymentId,
    Guid InvoiceId,
    decimal InvoiceTotal,
    decimal PaidNow,
    decimal ChangeAmount,
    string InvoiceInternalStatus
);
