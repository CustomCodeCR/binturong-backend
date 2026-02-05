using SharedKernel;

namespace Domain.Scopes;

public sealed record ScopeCreatedDomainEvent(Guid ScopeId, string Code, string? Description)
    : IDomainEvent;

public sealed record ScopeUpdatedDomainEvent(Guid ScopeId, string Code, string? Description)
    : IDomainEvent;

public sealed record ScopeDeletedDomainEvent(Guid ScopeId) : IDomainEvent;
