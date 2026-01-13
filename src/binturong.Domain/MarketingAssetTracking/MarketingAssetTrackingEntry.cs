using SharedKernel;

namespace Domain.MarketingAssetTracking;

public sealed class MarketingAssetTrackingEntry : Entity
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid? WebClientId { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? UserId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string MetadataJson { get; set; } = string.Empty;

    public Domain.MarketingAssets.MarketingAsset? Asset { get; set; }
    public Domain.WebClients.WebClient? WebClient { get; set; }
    public Domain.Clients.Client? Client { get; set; }
    public Domain.Users.User? User { get; set; }
}
