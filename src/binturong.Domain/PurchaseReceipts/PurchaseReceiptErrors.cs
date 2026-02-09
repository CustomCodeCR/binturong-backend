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
        "At least one receipt line is required"
    );

    public static readonly Error LineQuantityRequired = Error.Validation(
        "PurchaseReceipts.LineQuantityRequired",
        "Each line must have QuantityReceived > 0"
    );

    public static Error NotFound(Guid id) =>
        Error.NotFound("PurchaseReceipts.NotFound", $"Purchase receipt '{id}' not found");
}
