using SharedKernel;

namespace Domain.PurchaseRequests;

public static class PurchaseRequestErrors
{
    public static Error NotFound(Guid requestId) =>
        Error.NotFound(
            "PurchaseRequests.NotFound",
            $"The purchase request with the Id = '{requestId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "PurchaseRequests.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error CodeNotUnique = Error.Conflict(
        "PurchaseRequests.CodeNotUnique",
        "The provided purchase request code is not unique"
    );
}
