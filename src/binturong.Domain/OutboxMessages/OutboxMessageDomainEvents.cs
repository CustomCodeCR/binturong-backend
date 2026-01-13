using SharedKernel;

namespace Domain.OutboxMessages;

public sealed record OutboxMessageCreatedDomainEvent(Guid OutboxId) : IDomainEvent;
