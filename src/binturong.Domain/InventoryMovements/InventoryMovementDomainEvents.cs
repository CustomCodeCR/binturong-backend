using SharedKernel;

namespace Domain.InventoryMovements;

public sealed record InventoryMovementCreatedDomainEvent(Guid MovementId) : IDomainEvent;
