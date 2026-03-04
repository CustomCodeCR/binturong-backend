namespace Api.Endpoints.Payables;

public sealed record RegisterAccountsPayablePaymentRequest(
    decimal Amount,
    DateTime PaidAtUtc,
    string? Notes
);

public sealed record CreateAccountsPayableRequest(
    Guid SupplierId,
    Guid? PurchaseOrderId,
    string SupplierInvoiceId,
    DateOnly DocumentDate,
    DateOnly DueDate,
    decimal TotalAmount,
    string Currency
);
