using SharedKernel;

namespace Domain.InventoryMovements;

public static class InventoryMovementErrors
{
    public static Error NotFound(Guid movementId) =>
        Error.NotFound(
            "InventoryMovements.NotFound",
            $"The inventory movement with the Id = '{movementId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "InventoryMovements.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error InvalidQuantity = Error.Validation(
        "InventoryMovements.InvalidQuantity",
        "Quantity must be greater than zero"
    );

    public static readonly Error InvalidWarehouses = Error.Validation(
        "InventoryMovements.InvalidWarehouses",
        "WarehouseFrom and WarehouseTo cannot be the same"
    );
}
