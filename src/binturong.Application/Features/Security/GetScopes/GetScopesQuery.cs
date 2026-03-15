using Application.Abstractions.Messaging;
using Application.ReadModels.Security;

namespace Application.Features.Security.GetScopes;

public sealed record GetScopesQuery(int Page = 1, int PageSize = 50, string? Search = null)
    : IQuery<IReadOnlyList<ScopeCatalogReadModel>>
{
    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}
