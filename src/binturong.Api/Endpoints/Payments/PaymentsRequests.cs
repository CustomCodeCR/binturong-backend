namespace Api.Endpoints.Payments;

public sealed record RegisterCashPaymentRequest(
    Guid InvoiceId,
    Guid ClientId,
    Guid PaymentMethodId,
    DateTime PaymentDate,
    decimal AmountTendered,
    string? Notes
);

public sealed record RegisterTransferPaymentRequest(
    Guid InvoiceId,
    Guid ClientId,
    Guid PaymentMethodId,
    DateTime PaymentDate,
    decimal Amount,
    string Reference,
    bool IsBankConfirmed,
    string? Notes
);

public sealed record RegisterCardPaymentRequest(
    Guid InvoiceId,
    Guid ClientId,
    Guid PaymentMethodId,
    DateTime PaymentDate,
    decimal Amount,
    bool IsPosAvailable,
    bool IsApproved,
    string? PosAuthCode,
    string? Notes
);

public sealed record RegisterPartialPaymentRequest(
    Guid InvoiceId,
    Guid ClientId,
    Guid PaymentMethodId,
    DateTime PaymentDate,
    decimal Amount,
    string? Reference,
    string? Notes
);
