using SharedKernel;

namespace Domain.InventoryMovementTypes;

public sealed record InventoryMovementTypeCreatedDomainEvent(Guid MovementTypeId) : IDomainEvent;
