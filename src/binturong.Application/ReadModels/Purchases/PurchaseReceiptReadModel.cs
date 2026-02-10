using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Application.ReadModels.Purchases;

[BsonIgnoreExtraElements]
public sealed class PurchaseReceiptReadModel
{
    // ✅ Maps to Mongo "_id" as STRING (not ObjectId)
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; init; } = default!; // "purchase_receipt:{ReceiptId}"

    public Guid ReceiptId { get; init; }

    public Guid PurchaseOrderId { get; init; }
    public string PurchaseOrderCode { get; init; } = default!;

    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = default!;

    public DateTime ReceiptDate { get; init; }
    public string Status { get; init; } = default!;
    public string? Notes { get; init; }

    public IReadOnlyList<PurchaseReceiptLineReadModel> Lines { get; init; } =
        new List<PurchaseReceiptLineReadModel>();
}

public sealed class PurchaseReceiptLineReadModel
{
    public Guid ReceiptDetailId { get; init; }

    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;

    public decimal QuantityReceived { get; init; }
    public decimal UnitCost { get; init; }
}
