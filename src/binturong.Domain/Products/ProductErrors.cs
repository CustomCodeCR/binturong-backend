using SharedKernel;

namespace Domain.Products;

public static class ProductErrors
{
    public static Error NotFound(Guid productId) =>
        Error.NotFound("Products.NotFound", $"Product '{productId}' not found");

    public static Error Unauthorized() =>
        Error.Failure("Products.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error SkuNotUnique = Error.Conflict(
        "Products.SkuNotUnique",
        "The provided SKU is not unique"
    );

    public static readonly Error BarcodeNotUnique = Error.Conflict(
        "Products.BarcodeNotUnique",
        "The provided barcode is not unique"
    );

    public static readonly Error InvalidPrice = Error.Validation(
        "Products.InvalidPrice",
        "BasePrice must be >= 0"
    );

    public static readonly Error InvalidAverageCost = Error.Validation(
        "Products.InvalidAverageCost",
        "AverageCost must be >= 0"
    );
}
