using Application.Abstractions.Messaging;

namespace Application.Features.Payments.RegisterPartial;

public sealed record RegisterPartialPaymentCommand(
    Guid InvoiceId,
    Guid ClientId,
    Guid PaymentMethodId,
    DateTime PaymentDate,
    decimal Amount,
    string? Reference,
    string? Notes
) : ICommand<Guid>;
