using SharedKernel;

namespace Domain.AccountsPayable;

public sealed record AccountPayableCreatedDomainEvent(
    Guid AccountPayableId,
    Guid SupplierId,
    Guid? PurchaseOrderId,
    string SupplierInvoiceId,
    DateOnly DocumentDate,
    DateOnly DueDate,
    decimal TotalAmount,
    decimal PendingBalance,
    string Currency,
    string Status
) : IDomainEvent;

public sealed record AccountPayablePaymentRegisteredDomainEvent(
    Guid AccountPayableId,
    decimal Amount,
    decimal BalanceBefore,
    decimal BalanceAfter,
    string StatusAfter,
    DateTime PaidAtUtc,
    string? Notes
) : IDomainEvent;
