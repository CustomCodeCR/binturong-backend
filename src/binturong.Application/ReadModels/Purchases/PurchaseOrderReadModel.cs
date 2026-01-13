namespace Application.ReadModels.Purchases;

public sealed class PurchaseOrderReadModel
{
    public string Id { get; init; } = default!; // "purchase_order:{PurchaseOrderId}"
    public int PurchaseOrderId { get; init; }

    public string Code { get; init; } = default!;

    public int SupplierId { get; init; }
    public string SupplierName { get; init; } = default!;

    public int? BranchId { get; init; }
    public string? BranchName { get; init; }

    public int? RequestId { get; init; }

    public DateTime OrderDate { get; init; }
    public string Status { get; init; } = default!;

    public string Currency { get; init; } = default!;
    public decimal ExchangeRate { get; init; }

    public decimal Subtotal { get; init; }
    public decimal Taxes { get; init; }
    public decimal Discounts { get; init; }
    public decimal Total { get; init; }

    public IReadOnlyList<PurchaseOrderLineReadModel> Lines { get; init; } = [];
}

public sealed class PurchaseOrderLineReadModel
{
    public int PurchaseOrderDetailId { get; init; }

    public int ProductId { get; init; }
    public string ProductName { get; init; } = default!;

    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountPerc { get; init; }
    public decimal TaxPerc { get; init; }
    public decimal LineTotal { get; init; }
}
