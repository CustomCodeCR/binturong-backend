using SharedKernel;

namespace Domain.MarketingCampaigns;

public sealed record MarketingCampaignCreatedDomainEvent(Guid CampaignId) : IDomainEvent;
