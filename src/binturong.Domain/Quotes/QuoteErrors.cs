using SharedKernel;

namespace Domain.Quotes;

public static class QuoteErrors
{
    public static readonly Error ClientRequired = Error.Validation(
        "Quotes.ClientRequired",
        "Client is required"
    );

    public static readonly Error NoDetails = Error.Validation(
        "Quotes.NoDetails",
        "Quote must have at least one line"
    );

    public static readonly Error InvalidPrice = Error.Validation(
        "Quotes.InvalidPrice",
        "All lines must have a price"
    );

    public static readonly Error InvalidState = Error.Conflict(
        "Quotes.InvalidState",
        "Invalid quote state for this action"
    );

    public static readonly Error ClientEmailMissing = Error.Validation(
        "Quotes.ClientEmailMissing",
        "Client email is missing"
    );
}
