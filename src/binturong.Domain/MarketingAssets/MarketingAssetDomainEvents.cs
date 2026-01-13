using SharedKernel;

namespace Domain.MarketingAssets;

public sealed record MarketingAssetPublishedDomainEvent(Guid AssetId) : IDomainEvent;
