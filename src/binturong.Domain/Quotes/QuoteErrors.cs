using SharedKernel;

namespace Domain.Quotes;

public static class QuoteErrors
{
    public static Error NotFound(Guid quoteId) =>
        Error.NotFound("Quotes.NotFound", $"The quote with the Id = '{quoteId}' was not found");

    public static Error Unauthorized() =>
        Error.Failure("Quotes.Unauthorized", "You are not authorized to perform this action.");

    public static readonly Error CodeNotUnique = Error.Conflict(
        "Quotes.CodeNotUnique",
        "The provided quote code is not unique"
    );
}
