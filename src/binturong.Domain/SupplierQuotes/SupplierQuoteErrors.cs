using SharedKernel;

namespace Domain.SupplierQuotes;

public static class SupplierQuoteErrors
{
    public static readonly Error CodeRequired = Error.Validation(
        "SupplierQuotes.CodeRequired",
        "Code is required."
    );

    public static readonly Error SupplierRequired = Error.Validation(
        "SupplierQuotes.SupplierRequired",
        "SupplierId is required."
    );

    public static readonly Error NoLines = Error.Validation(
        "SupplierQuotes.NoLines",
        "At least one line is required."
    );

    public static readonly Error LineQuantityInvalid = Error.Validation(
        "SupplierQuotes.LineQuantityInvalid",
        "Line Quantity must be > 0."
    );

    public static readonly Error InvalidStatusTransition = Error.Validation(
        "SupplierQuotes.InvalidStatusTransition",
        "Invalid status transition."
    );

    public static readonly Error ResponseLineUnitPriceInvalid = Error.Validation(
        "SupplierQuotes.ResponseLineUnitPriceInvalid",
        "UnitPrice must be > 0."
    );

    public static readonly Error RejectReasonRequired = Error.Validation(
        "SupplierQuotes.RejectReasonRequired",
        "Reject reason is required."
    );

    public static Error NotFound(Guid id) =>
        Error.NotFound("SupplierQuotes.NotFound", $"Supplier quote '{id}' not found.");
}
