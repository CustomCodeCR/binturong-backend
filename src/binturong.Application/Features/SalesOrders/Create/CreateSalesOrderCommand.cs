using Application.Abstractions.Messaging;

namespace Application.Features.SalesOrders.Create;

public sealed record CreateSalesOrderLine(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc
);

public sealed record CreateSalesOrderCommand(
    Guid ClientId,
    Guid? BranchId,
    Guid? SellerUserId,
    string Currency,
    decimal ExchangeRate,
    string? Notes,
    IReadOnlyList<CreateSalesOrderLine> Lines
) : ICommand<Guid>;
