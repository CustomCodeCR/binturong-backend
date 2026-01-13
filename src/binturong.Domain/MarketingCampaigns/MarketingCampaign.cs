using SharedKernel;

namespace Domain.MarketingCampaigns;

public sealed class MarketingCampaign : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string TargetArea { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Domain.MarketingAudienceRules.MarketingAudienceRule> AudienceRules { get; set; } =
        new List<Domain.MarketingAudienceRules.MarketingAudienceRule>();

    public ICollection<Domain.MarketingAssets.MarketingAsset> Assets { get; set; } =
        new List<Domain.MarketingAssets.MarketingAsset>();
}
