using SharedKernel;

namespace Domain.PurchaseReceipts;

public sealed record PurchaseReceiptRegisteredDomainEvent(
    Guid ReceiptId,
    Guid PurchaseOrderId,
    Guid WarehouseId,
    DateTime ReceiptDate,
    string Status,
    string? Notes
) : IDomainEvent;

public sealed record PurchaseReceiptDetailAddedDomainEvent(
    Guid ReceiptId,
    Guid ReceiptDetailId,
    Guid ProductId,
    decimal QuantityReceived,
    decimal UnitCost
) : IDomainEvent;

public sealed record PurchaseReceiptRejectedDomainEvent(
    Guid ReceiptId,
    Guid PurchaseOrderId,
    string Reason,
    DateTime RejectedAtUtc
) : IDomainEvent;
