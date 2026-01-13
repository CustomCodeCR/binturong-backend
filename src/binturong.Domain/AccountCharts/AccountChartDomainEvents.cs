using SharedKernel;

namespace Domain.AccountsChart;

public sealed record AccountChartCreatedDomainEvent(Guid AccountId) : IDomainEvent;
