using SharedKernel;

namespace Domain.InventoryTransfers;

public static class InventoryTransferErrors
{
    public static Error NotFound(Guid transferId) =>
        Error.NotFound("InventoryTransfers.NotFound", $"Transfer '{transferId}' not found");

    public static readonly Error EmptyLines = Error.Validation(
        "InventoryTransfers.EmptyLines",
        "Transfer must have at least one line"
    );

    public static readonly Error InvalidStatus = Error.Failure(
        "InventoryTransfers.InvalidStatus",
        "Transfer status does not allow this action"
    );

    public static readonly Error RequiresApproval = Error.Failure(
        "InventoryTransfers.RequiresApproval",
        "Transfer requires approval before confirmation"
    );

    public static Error StockInsufficient(Guid productId, Guid warehouseId) =>
        Error.Failure(
            "InventoryTransfers.StockInsufficient",
            $"Insufficient stock for product '{productId}' in warehouse '{warehouseId}'"
        );

    public static readonly Error InvalidBranches = Error.Validation(
        "InventoryTransfers.InvalidBranches",
        "FromBranchId and ToBranchId must be different"
    );
}
