using SharedKernel;

namespace Domain.UserScopes;

public sealed record UserScopeAssignedDomainEvent(Guid UserId, Guid ScopeId, string ScopeCode)
    : IDomainEvent;

public sealed record UserScopeRemovedDomainEvent(Guid UserId, Guid ScopeId, string ScopeCode)
    : IDomainEvent;
