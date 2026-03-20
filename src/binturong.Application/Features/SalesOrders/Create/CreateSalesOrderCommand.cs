using Application.Abstractions.Messaging;

namespace Application.Features.SalesOrders.Create;

public sealed record CreateSalesOrderCommand(
    Guid ClientId,
    Guid? BranchId,
    Guid? SellerUserId,
    string Currency,
    decimal ExchangeRate,
    string? Notes,
    IReadOnlyList<CreateSalesOrderLine> Lines
) : ICommand<Guid>;

public sealed record CreateSalesOrderLine(
    string ItemType, // Product | Service
    Guid ItemId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc
);
