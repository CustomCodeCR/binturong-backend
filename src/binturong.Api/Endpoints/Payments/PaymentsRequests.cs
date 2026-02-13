namespace Api.Endpoints.Payments;

public sealed record RegisterPaymentAppliedInvoiceRequest(Guid InvoiceId, decimal AppliedAmount);

public sealed record RegisterPaymentRequest(
    Guid ClientId,
    Guid PaymentMethodId,
    DateTime PaymentDate,
    decimal TotalAmount,
    string? Reference,
    string? Notes,
    IReadOnlyList<RegisterPaymentAppliedInvoiceRequest> AppliedInvoices
);
