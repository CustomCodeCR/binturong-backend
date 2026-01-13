using SharedKernel;

namespace Domain.WebClients;

public sealed record WebClientRegisteredDomainEvent(Guid WebClientId) : IDomainEvent;
