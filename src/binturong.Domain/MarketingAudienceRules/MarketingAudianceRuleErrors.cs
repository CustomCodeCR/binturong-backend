using SharedKernel;

namespace Domain.MarketingAudienceRules;

public static class MarketingAudienceRuleErrors
{
    public static Error NotFound(Guid ruleId) =>
        Error.NotFound(
            "MarketingAudienceRules.NotFound",
            $"The audience rule with the Id = '{ruleId}' was not found"
        );

    public static Error Unauthorized() =>
        Error.Failure(
            "MarketingAudienceRules.Unauthorized",
            "You are not authorized to perform this action."
        );
}
