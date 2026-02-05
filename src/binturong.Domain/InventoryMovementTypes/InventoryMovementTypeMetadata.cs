namespace Domain.InventoryMovementTypes;

public static class InventoryMovementTypeMetadata
{
    public static string Code(this InventoryMovementType type) =>
        type switch
        {
            InventoryMovementType.PurchaseIn => "PURCHASE_IN",
            InventoryMovementType.SaleOut => "SALE_OUT",
            InventoryMovementType.TransferIn => "TRANSFER_IN",
            InventoryMovementType.TransferOut => "TRANSFER_OUT",
            InventoryMovementType.AdjustmentIn => "ADJUSTMENT_IN",
            InventoryMovementType.AdjustmentOut => "ADJUSTMENT_OUT",
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    public static string Description(this InventoryMovementType type) =>
        type switch
        {
            InventoryMovementType.PurchaseIn => "Purchase entry",
            InventoryMovementType.SaleOut => "Sale output",
            InventoryMovementType.TransferIn => "Warehouse transfer in",
            InventoryMovementType.TransferOut => "Warehouse transfer out",
            InventoryMovementType.AdjustmentIn => "Positive adjustment",
            InventoryMovementType.AdjustmentOut => "Negative adjustment",
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

    public static int Sign(this InventoryMovementType type) =>
        type switch
        {
            InventoryMovementType.PurchaseIn => +1,
            InventoryMovementType.TransferIn => +1,
            InventoryMovementType.AdjustmentIn => +1,

            InventoryMovementType.SaleOut => -1,
            InventoryMovementType.TransferOut => -1,
            InventoryMovementType.AdjustmentOut => -1,

            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
}
