namespace Api.Endpoints.Payables;

public sealed record RegisterAccountsPayablePaymentRequest(
    decimal Amount,
    DateTime PaidAtUtc,
    string? Notes
);
