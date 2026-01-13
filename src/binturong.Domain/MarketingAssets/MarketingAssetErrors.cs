using SharedKernel;

namespace Domain.MarketingAssets;

public static class MarketingAssetErrors
{
    public static Error NotFound(Guid assetId) =>
        Error.NotFound(
            "MarketingAssets.NotFound",
            $"The marketing asset with the Id = '{assetId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "MarketingAssets.Unauthorized",
            "You are not authorized to perform this action."
        );
}
