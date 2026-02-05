using SharedKernel;

namespace Domain.Inventory;

public static class InventoryErrors
{
    public static Error WarehouseNotFound(Guid warehouseId) =>
        Error.NotFound("Warehouses.NotFound", $"Warehouse '{warehouseId}' not found");

    public static Error ProductNotFound(Guid productId) =>
        Error.NotFound("Products.NotFound", $"Product '{productId}' not found");

    public static readonly Error InvalidQuantity = Error.Validation(
        "Inventory.InvalidQuantity",
        "Quantity must be greater than 0"
    );

    public static readonly Error InvalidCountedStock = Error.Validation(
        "Inventory.InvalidCountedStock",
        "Counted stock must be >= 0"
    );
}
