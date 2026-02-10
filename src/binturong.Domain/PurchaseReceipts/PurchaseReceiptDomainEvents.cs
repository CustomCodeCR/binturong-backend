using SharedKernel;

namespace Domain.PurchaseReceipts;

public sealed record PurchaseReceiptRejectedDomainEvent(
    Guid ReceiptId,
    Guid PurchaseOrderId,
    DateTime RejectedAtUtc,
    string Reason,
    string Status
) : IDomainEvent;

public sealed record PurchaseReceiptLineAddedDomainEvent(
    Guid ReceiptId,
    Guid ReceiptDetailId,
    Guid ProductId,
    decimal QuantityReceived,
    decimal UnitCost
) : IDomainEvent;

public sealed record PurchaseReceiptCreatedDomainEvent(
    Guid ReceiptId,
    Guid PurchaseOrderId,
    Guid WarehouseId,
    DateTime ReceiptDateUtc,
    string Status,
    string? Notes
) : IDomainEvent;
