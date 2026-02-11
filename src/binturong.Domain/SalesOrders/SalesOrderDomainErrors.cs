using SharedKernel;

namespace Domain.SalesOrders;

public sealed record SalesOrderCreatedDomainEvent(
    Guid SalesOrderId,
    string Code,
    Guid ClientId,
    Guid? BranchId,
    DateTime OrderDateUtc,
    string Status,
    string Currency,
    decimal ExchangeRate,
    decimal Subtotal,
    decimal Taxes,
    decimal Discounts,
    decimal Total,
    Guid? QuoteId
) : IDomainEvent;

public sealed record SalesOrderConfirmedDomainEvent(
    Guid SalesOrderId,
    Guid SellerUserId,
    decimal Total
) : IDomainEvent;

public sealed record SalesOrderConvertedFromQuoteDomainEvent(Guid SalesOrderId, Guid QuoteId)
    : IDomainEvent;

public sealed record SalesOrderDetailAddedDomainEvent(
    Guid SalesOrderId,
    Guid SalesOrderDetailId,
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc,
    decimal LineTotal
) : IDomainEvent;
