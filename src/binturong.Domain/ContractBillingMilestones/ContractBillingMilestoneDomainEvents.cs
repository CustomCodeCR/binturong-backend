using SharedKernel;

namespace Domain.ContractBillingMilestones;

public sealed record ContractMilestoneAddedDomainEvent(
    Guid ContractId,
    Guid MilestoneId,
    string Description,
    decimal Percentage,
    decimal Amount,
    DateOnly ScheduledDate,
    bool IsBilled,
    Guid? InvoiceId
) : IDomainEvent;

public sealed record ContractMilestoneUpdatedDomainEvent(
    Guid ContractId,
    Guid MilestoneId,
    string Description,
    decimal Percentage,
    decimal Amount,
    DateOnly ScheduledDate,
    bool IsBilled,
    Guid? InvoiceId
) : IDomainEvent;

public sealed record ContractMilestoneRemovedDomainEvent(Guid ContractId, Guid MilestoneId)
    : IDomainEvent;
