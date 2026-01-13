using SharedKernel;

namespace Domain.ContractBillingMilestones;

public sealed record ContractBillingMilestoneCreatedDomainEvent(Guid MilestoneId) : IDomainEvent;
