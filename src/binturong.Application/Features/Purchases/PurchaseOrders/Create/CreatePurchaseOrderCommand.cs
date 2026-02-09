using Application.Abstractions.Messaging;

namespace Application.Features.Purchases.PurchaseOrders.Create;

public sealed record CreatePurchaseOrderCommand(
    string Code,
    Guid SupplierId,
    Guid? BranchId,
    Guid? RequestId,
    DateTime OrderDateUtc,
    string Currency,
    decimal ExchangeRate,
    IReadOnlyList<CreatePurchaseOrderLineDto> Lines
) : ICommand<Guid>;

public sealed record CreatePurchaseOrderLineDto(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc
);
