using SharedKernel;

namespace Domain.SalesOrders;

public static class SalesOrderErrors
{
    public static readonly Error ClientRequired = Error.Validation(
        "SalesOrders.ClientRequired",
        "ClientId is required."
    );

    public static readonly Error DetailsRequired = Error.Validation(
        "SalesOrders.DetailsRequired",
        "At least one detail line is required."
    );

    public static Error NotFound(Guid id) =>
        Error.NotFound("SalesOrders.NotFound", $"Sales order '{id}' not found.");

    public static Error QuoteNotFound(Guid id) =>
        Error.NotFound("SalesOrders.QuoteNotFound", $"Quote '{id}' not found.");

    public static Error QuoteNotApproved(Guid id) =>
        Error.Conflict("SalesOrders.QuoteNotApproved", $"Quote '{id}' is not approved.");

    public static Error QuoteExpired(Guid id, DateTime validUntilUtc) =>
        Error.Conflict(
            "SalesOrders.QuoteExpired",
            $"Quote '{id}' expired at {validUntilUtc:O}. Revalidate before converting."
        );

    public static readonly Error SellerRequiredForCommission = Error.Validation(
        "SalesOrders.SellerRequiredForCommission",
        "SellerUserId is required."
    );

    public static readonly Error AlreadyConfirmed = Error.Conflict(
        "SalesOrders.AlreadyConfirmed",
        "Sales order is already confirmed."
    );

    public static Error QuoteNotAccepted(Guid id) =>
        Error.Conflict("SalesOrders.QuoteNotAccepted", $"Quote '{id}' is not accepted by client.");

    public static Error QuoteAlreadyConverted(Guid id) =>
        Error.Conflict(
            "SalesOrders.QuoteAlreadyConverted",
            $"Quote '{id}' was already converted to a sales order."
        );
}
