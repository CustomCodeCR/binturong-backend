namespace Application.Features.Payments.Reports;

public sealed record PaymentHistoryRow(
    DateTime PaymentDate,
    string PaymentMethod,
    string ClientName,
    decimal TotalAmount,
    string? Reference,
    string? Notes,
    string AppliedInvoices // texto simple: "INV-001 (5000), INV-002 (2500)"
);
