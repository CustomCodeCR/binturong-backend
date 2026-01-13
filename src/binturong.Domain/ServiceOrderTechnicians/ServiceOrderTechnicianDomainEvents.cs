using SharedKernel;

namespace Domain.ServiceOrderTechnicians;

public sealed record TechnicianAssignedToServiceOrderDomainEvent(Guid ServiceOrderTechId)
    : IDomainEvent;
