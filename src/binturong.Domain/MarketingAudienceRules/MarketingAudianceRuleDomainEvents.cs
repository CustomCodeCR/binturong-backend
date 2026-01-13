using SharedKernel;

namespace Domain.MarketingAudienceRules;

public sealed record MarketingAudienceRuleCreatedDomainEvent(Guid RuleId) : IDomainEvent;
