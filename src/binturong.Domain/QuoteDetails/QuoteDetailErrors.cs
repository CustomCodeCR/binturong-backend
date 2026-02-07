using SharedKernel;

namespace Domain.QuoteDetails;

public static class QuoteDetailErrors
{
    public static readonly Error QuantityInvalid = Error.Validation(
        "QuoteDetails.QuantityInvalid",
        "Quantity must be greater than zero"
    );

    public static readonly Error PriceInvalid = Error.Validation(
        "QuoteDetails.PriceInvalid",
        "Unit price must be greater than zero"
    );

    public static readonly Error NoLines = Error.Validation(
        "QuoteDetails.NoLines",
        "At least one line is required"
    );
}
