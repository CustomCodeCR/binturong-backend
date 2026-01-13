using SharedKernel;

namespace Domain.MarketingAudienceRules;

public sealed class MarketingAudienceRule : Entity
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string RuleValue { get; set; } = string.Empty;

    public Domain.MarketingCampaigns.MarketingCampaign? Campaign { get; set; }
}
