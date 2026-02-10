using SharedKernel;

namespace Domain.PurchaseReceipts;

public static class PurchaseReceiptErrors
{
    public static readonly Error PurchaseOrderRequired = Error.Validation(
        "PurchaseReceipts.PurchaseOrderRequired",
        "PurchaseOrderId is required"
    );

    public static readonly Error WarehouseRequired = Error.Validation(
        "PurchaseReceipts.WarehouseRequired",
        "WarehouseId is required"
    );

    public static readonly Error NoLines = Error.Validation(
        "PurchaseReceipts.NoLines",
        "At least one line is required"
    );

    public static readonly Error ProductRequired = Error.Validation(
        "PurchaseReceipts.ProductRequired",
        "All lines must have ProductId"
    );

    public static readonly Error QuantityInvalid = Error.Validation(
        "PurchaseReceipts.QuantityInvalid",
        "All lines must have QuantityReceived > 0"
    );

    public static readonly Error UnitCostInvalid = Error.Validation(
        "PurchaseReceipts.UnitCostInvalid",
        "All lines must have UnitCost > 0"
    );

    public static readonly Error RejectReasonRequired = Error.Validation(
        "PurchaseReceipts.RejectReasonRequired",
        "Reason is required"
    );

    public static Error ProductNotInOrder(Guid productId, Guid purchaseOrderId) =>
        Error.Validation(
            "PurchaseReceipts.ProductNotInOrder",
            $"Product '{productId}' is not part of purchase order '{purchaseOrderId}'"
        );

    public static Error QuantityExceedsOrdered(Guid productId, decimal received, decimal ordered) =>
        Error.Validation(
            "PurchaseReceipts.QuantityExceedsOrdered",
            $"Received qty for product '{productId}' exceeds ordered qty ({received} > {ordered})"
        );
}
