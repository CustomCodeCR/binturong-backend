namespace Application.ReadModels.Sales;

public sealed class SalesOrderReadModel
{
    public string Id { get; init; } = default!; // "so:{SalesOrderId}"
    public int SalesOrderId { get; init; }
    public string Code { get; init; } = default!;

    public int? QuoteId { get; init; }

    public int ClientId { get; init; }
    public string ClientName { get; init; } = default!;

    public int? BranchId { get; init; }
    public string? BranchName { get; init; }

    public DateTime OrderDate { get; init; }
    public string Status { get; init; } = default!;

    public string Currency { get; init; } = default!;
    public decimal ExchangeRate { get; init; }

    public decimal Subtotal { get; init; }
    public decimal Taxes { get; init; }
    public decimal Discounts { get; init; }
    public decimal Total { get; init; }

    public string? Notes { get; init; }

    public IReadOnlyList<SalesOrderLineReadModel> Lines { get; init; } = [];
}

public sealed class SalesOrderLineReadModel
{
    public int SalesOrderDetailId { get; init; }

    public int ProductId { get; init; }
    public string ProductName { get; init; } = default!;

    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountPerc { get; init; }
    public decimal TaxPerc { get; init; }
    public decimal LineTotal { get; init; }
}
