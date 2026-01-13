using SharedKernel;

namespace Domain.Scopes;

public sealed record ScopeCreatedDomainEvent(Guid ScopeId) : IDomainEvent;
