namespace Api.Endpoints.Purchases;

public sealed record CreatePurchaseOrderRequest(
    string Code,
    Guid SupplierId,
    Guid? BranchId,
    Guid? RequestId,
    DateTime OrderDateUtc,
    string Currency,
    decimal ExchangeRate,
    List<CreatePurchaseOrderLineRequest> Lines
);

public sealed record CreatePurchaseOrderLineRequest(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc
);
