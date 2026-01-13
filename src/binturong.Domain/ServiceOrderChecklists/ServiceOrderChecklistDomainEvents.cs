using SharedKernel;

namespace Domain.ServiceOrderChecklists;

public sealed record ServiceOrderChecklistItemCreatedDomainEvent(Guid ChecklistId) : IDomainEvent;
