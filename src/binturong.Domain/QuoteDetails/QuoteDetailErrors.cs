using SharedKernel;

namespace Domain.QuoteDetails;

public static class QuoteDetailErrors
{
    public static Error NotFound(Guid quoteDetailId) =>
        Error.NotFound(
            "QuoteDetails.NotFound",
            $"The quote detail with the Id = '{quoteDetailId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "QuoteDetails.Unauthorized",
            "You are not authorized to perform this action."
        );
}
