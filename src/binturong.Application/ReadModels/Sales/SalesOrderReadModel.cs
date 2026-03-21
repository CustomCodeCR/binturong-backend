using MongoDB.Bson.Serialization.Attributes;

namespace Application.ReadModels.Sales;

public sealed class SalesOrderReadModel
{
    public string Id { get; init; } = default!;
    public Guid SalesOrderId { get; init; }
    public string Code { get; init; } = default!;

    public Guid? QuoteId { get; init; }

    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = default!;

    public Guid? BranchId { get; init; }
    public string? BranchName { get; init; }

    public Guid? SellerUserId { get; init; }
    public string? SellerName { get; init; }

    public DateTime OrderDate { get; init; }
    public string Status { get; init; } = default!;

    public string Currency { get; init; } = default!;
    public decimal ExchangeRate { get; init; }

    public decimal Subtotal { get; init; }
    public decimal Taxes { get; init; }
    public decimal Discounts { get; init; }
    public decimal Total { get; init; }

    public decimal GlobalDiscountPerc { get; init; }
    public decimal GlobalDiscountAmount { get; init; }
    public string? GlobalDiscountReason { get; init; }

    public string? Notes { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public IReadOnlyList<SalesOrderLineReadModel> Lines { get; init; } = [];
}

[BsonIgnoreExtraElements]
public sealed class SalesOrderLineReadModel
{
    public Guid SalesOrderDetailId { get; init; }

    public string ItemType { get; init; } = default!;

    public Guid? ProductId { get; init; }
    public Guid? ServiceId { get; init; }

    public string ItemName { get; init; } = default!;

    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }

    public decimal DiscountPerc { get; init; }
    public decimal DiscountAmount { get; init; }
    public string? DiscountReason { get; init; }

    public decimal TaxPerc { get; init; }
    public decimal LineTotal { get; init; }
}
