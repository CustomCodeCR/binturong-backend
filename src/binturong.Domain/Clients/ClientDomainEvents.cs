using SharedKernel;

namespace Domain.Clients;

public sealed record ClientCreatedDomainEvent(Guid ClientId) : IDomainEvent;
