using SharedKernel;

namespace Domain.MarketingAssetTracking;

public sealed record MarketingAssetTrackedDomainEvent(Guid TrackingId) : IDomainEvent;
