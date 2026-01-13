using SharedKernel;

namespace Domain.ServiceOrderMaterials;

public sealed record MaterialAddedToServiceOrderDomainEvent(Guid ServiceOrderMaterialId)
    : IDomainEvent;
