namespace Application.ReadModels.Marketing;

public sealed class MarketingCampaignReadModel
{
    public string Id { get; init; } = default!; // "campaign:{CampaignId}"
    public int CampaignId { get; init; }

    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; }

    public DateTime StartsAt { get; init; }
    public DateTime EndsAt { get; init; }

    public string Status { get; init; } = default!;
    public int Priority { get; init; }
    public string? TargetArea { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public IReadOnlyList<MarketingAudienceRuleReadModel> Rules { get; init; } = [];
    public IReadOnlyList<MarketingAssetReadModel> Assets { get; init; } = [];
}

public sealed class MarketingAudienceRuleReadModel
{
    public int RuleId { get; init; }
    public string RuleType { get; init; } = default!;
    public string RuleValue { get; init; } = default!;
}

public sealed class MarketingAssetReadModel
{
    public int AssetId { get; init; }
    public string AssetType { get; init; } = default!; // POPUP/BANNER/NEWS
    public string Title { get; init; } = default!;
    public string? Body { get; init; }
    public string? ImageS3Key { get; init; }
    public string? LinkUrl { get; init; }
    public string? RouteStartsWith { get; init; }

    public int MaxShowsPerSession { get; init; }
    public int CooldownMinutes { get; init; }

    public DateTime StartsAt { get; init; }
    public DateTime EndsAt { get; init; }
    public bool IsActive { get; init; }

    public int? PromotionId { get; init; }
}
