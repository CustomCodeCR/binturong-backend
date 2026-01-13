using SharedKernel;

namespace Domain.WarehouseStocks;

public static class WarehouseStockErrors
{
    public static Error NotFound(Guid stockId) =>
        Error.NotFound(
            "WarehouseStock.NotFound",
            $"The warehouse stock with the Id = '{stockId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "WarehouseStock.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error DuplicateWarehouseProduct = Error.Conflict(
        "WarehouseStock.DuplicateWarehouseProduct",
        "This product already has a stock record for the given warehouse"
    );
}
