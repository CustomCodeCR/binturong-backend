namespace Application.ReadModels.Marketing;

public sealed class MarketingAssetTrackingReadModel
{
    public string Id { get; init; } = default!; // "asset_track:{TrackingId}"
    public int TrackingId { get; init; }

    public int AssetId { get; init; }
    public string EventType { get; init; } = default!; // VIEW/CLICK/DISMISS

    public int? WebClientId { get; init; }
    public int? ClientId { get; init; }
    public int? UserId { get; init; }

    public string SessionId { get; init; } = default!;
    public DateTime EventDate { get; init; }

    public string? MetadataJson { get; init; }
}
