using Application.Abstractions.Messaging;

namespace Application.Features.Payments.Register;

public sealed record RegisterPaymentCommand(
    Guid ClientId,
    Guid PaymentMethodId,
    DateTime PaymentDate,
    decimal TotalAmount,
    string Reference,
    string Notes,
    IReadOnlyList<ApplyInvoicePayment> AppliedInvoices
) : ICommand<Guid>;

public sealed record ApplyInvoicePayment(Guid InvoiceId, decimal AppliedAmount);
