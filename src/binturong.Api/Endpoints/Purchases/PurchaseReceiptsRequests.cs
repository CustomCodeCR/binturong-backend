namespace Api.Endpoints.Purchases;

public sealed record CreatePurchaseReceiptRequest(
    Guid PurchaseOrderId,
    Guid WarehouseId,
    DateTime ReceiptDateUtc,
    string? Notes,
    List<CreatePurchaseReceiptLineRequest> Lines
);

public sealed record CreatePurchaseReceiptLineRequest(
    Guid ProductId,
    decimal QuantityReceived,
    decimal UnitCost
);

public sealed record RejectPurchaseReceiptRequest(string Reason, DateTime RejectedAtUtc);
