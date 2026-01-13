using SharedKernel;

namespace Domain.MarketingAssets;

public sealed class MarketingAsset : Entity
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string ImageS3Key { get; set; } = string.Empty;
    public string LinkUrl { get; set; } = string.Empty;
    public string RouteStartsWith { get; set; } = string.Empty;
    public int MaxShowsPerSession { get; set; }
    public int CooldownMinutes { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool IsActive { get; set; }
    public Guid? PromotionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Domain.MarketingCampaigns.MarketingCampaign? Campaign { get; set; }
    public ICollection<Domain.MarketingAssetTracking.MarketingAssetTrackingEntry> Trackings { get; set; } =
        new List<Domain.MarketingAssetTracking.MarketingAssetTrackingEntry>();
}
