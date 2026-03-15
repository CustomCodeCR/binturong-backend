namespace Application.Features.Security.BrowseScopes;

public sealed record BrowseSecurityScopesResponse(
    string Id,
    Guid ScopeId,
    string Code,
    string? Description
);
