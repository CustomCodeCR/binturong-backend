using SharedKernel;

namespace Domain.MarketingAssetTracking;

public static class MarketingAssetTrackingErrors
{
    public static Error NotFound(Guid trackingId) =>
        Error.NotFound(
            "MarketingAssetTracking.NotFound",
            $"The marketing asset tracking entry with the Id = '{trackingId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "MarketingAssetTracking.Unauthorized",
            "You are not authorized to perform this action."
        );
}
