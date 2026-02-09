using MongoDB.Bson.Serialization.Attributes;

namespace Application.ReadModels.Purchases;

[BsonIgnoreExtraElements]
public sealed class PurchaseOrderReadModel
{
    // ✅ Treat MongoDB _id as string (your logical id)
    [BsonId]
    public string Id { get; init; } = default!; // "purchase_order:{PurchaseOrderId}"

    public Guid PurchaseOrderId { get; init; }
    public string Code { get; init; } = default!;

    public Guid SupplierId { get; init; }
    public string SupplierName { get; init; } = default!;

    public Guid? BranchId { get; init; }
    public string? BranchName { get; init; }

    public Guid? RequestId { get; init; }

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
    public Guid PurchaseOrderDetailId { get; init; }

    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;

    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountPerc { get; init; }
    public decimal TaxPerc { get; init; }
    public decimal LineTotal { get; init; }
}
