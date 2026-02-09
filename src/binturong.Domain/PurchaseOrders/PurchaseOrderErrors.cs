using SharedKernel;

namespace Domain.PurchaseOrders;

public static class PurchaseOrderErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("PurchaseOrders.NotFound", $"Purchase order '{id}' not found");

    public static readonly Error CodeRequired = Error.Validation(
        "PurchaseOrders.CodeRequired",
        "Code is required"
    );

    public static readonly Error CodeNotUnique = Error.Conflict(
        "PurchaseOrders.CodeNotUnique",
        "The provided purchase order code is not unique"
    );

    public static readonly Error SupplierRequired = Error.Validation(
        "PurchaseOrders.SupplierRequired",
        "SupplierId is required"
    );

    public static readonly Error NoLines = Error.Validation(
        "PurchaseOrders.NoLines",
        "At least one line is required"
    );

    public static readonly Error ProductRequired = Error.Validation(
        "PurchaseOrders.ProductRequired",
        "All lines must have ProductId"
    );

    public static readonly Error QuantityRequired = Error.Validation(
        "PurchaseOrders.QuantityRequired",
        "All lines must have Quantity > 0"
    );

    public static readonly Error UnitPriceInvalid = Error.Validation(
        "PurchaseOrders.UnitPriceInvalid",
        "All lines must have UnitPrice > 0"
    );

    public static readonly Error CurrencyRequired = Error.Validation(
        "PurchaseOrders.CurrencyRequired",
        "Currency is required"
    );

    public static readonly Error ExchangeRateInvalid = Error.Validation(
        "PurchaseOrders.ExchangeRateInvalid",
        "ExchangeRate must be > 0"
    );
}
