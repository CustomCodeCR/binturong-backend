using SharedKernel;

namespace Domain.AccountingPeriods;

public sealed record AccountingPeriodCreatedDomainEvent(Guid PeriodId) : IDomainEvent;
