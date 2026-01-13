using SharedKernel;

namespace Domain.MarketingCampaigns;

public static class MarketingCampaignErrors
{
    public static Error NotFound(Guid campaignId) =>
        Error.NotFound(
            "MarketingCampaigns.NotFound",
            $"The marketing campaign with the Id = '{campaignId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "MarketingCampaigns.Unauthorized",
            "You are not authorized to perform this action."
        );

    public static readonly Error CodeNotUnique = Error.Conflict(
        "MarketingCampaigns.CodeNotUnique",
        "The provided campaign code is not unique"
    );
}
