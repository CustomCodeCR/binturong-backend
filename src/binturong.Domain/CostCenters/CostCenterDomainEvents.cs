using SharedKernel;

namespace Domain.CostCenters;

public sealed record CostCenterCreatedDomainEvent(Guid CostCenterId) : IDomainEvent;
