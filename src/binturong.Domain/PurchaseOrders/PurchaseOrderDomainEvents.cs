using SharedKernel;

namespace Domain.PurchaseOrders;

public sealed record PurchaseOrderCreatedDomainEvent(
    Guid PurchaseOrderId,
    string Code,
    Guid SupplierId,
    Guid? BranchId,
    Guid? RequestId,
    DateTime OrderDate,
    string Status,
    string Currency,
    decimal ExchangeRate,
    decimal Subtotal,
    decimal Taxes,
    decimal Discounts,
    decimal Total
) : IDomainEvent;

public sealed record PurchaseOrderStatusChangedDomainEvent(
    Guid PurchaseOrderId,
    string Status,
    DateTime ChangedAtUtc,
    string? Reason
) : IDomainEvent;

public sealed record PurchaseOrderLineAddedDomainEvent(
    Guid PurchaseOrderId,
    Guid PurchaseOrderDetailId,
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc,
    decimal LineTotal
) : IDomainEvent;
