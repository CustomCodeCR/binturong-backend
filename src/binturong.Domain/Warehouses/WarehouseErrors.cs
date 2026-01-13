using SharedKernel;

namespace Domain.Warehouses;

public static class WarehouseErrors
{
    public static Error NotFound(Guid warehouseId) =>
        Error.NotFound(
            "Warehouses.NotFound",
            $"The warehouse with the Id = '{warehouseId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure("Warehouses.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error CodeNotUnique = Error.Conflict(
        "Warehouses.CodeNotUnique",
        "The provided warehouse code is not unique"
    );
}
