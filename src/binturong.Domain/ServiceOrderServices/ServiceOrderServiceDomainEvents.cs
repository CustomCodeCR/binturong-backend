using SharedKernel;

namespace Domain.ServiceOrderServices;

public sealed record ServiceAddedToServiceOrderDomainEvent(Guid ServiceOrderServiceId)
    : IDomainEvent;
