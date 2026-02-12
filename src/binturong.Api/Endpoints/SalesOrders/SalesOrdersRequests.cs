namespace Api.Endpoints.SalesOrders;

public sealed record CreateSalesOrderLineRequest(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc
);

public sealed record CreateSalesOrderRequest(
    Guid ClientId,
    Guid? BranchId,
    Guid? SellerUserId,
    string Currency,
    decimal ExchangeRate,
    string? Notes,
    IReadOnlyList<CreateSalesOrderLineRequest> Lines
);

public sealed record ConvertQuoteToSalesOrderRequest(
    Guid? BranchId,
    string Currency,
    decimal ExchangeRate,
    string? Notes
);

public sealed record ConfirmSalesOrderRequest(Guid SellerUserId);
