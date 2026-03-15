using Application.Abstractions.Messaging;
using Application.ReadModels.Security;

namespace Application.Features.Security.GetScopes;

public sealed record GetScopesQuery(string? Search = null)
    : IQuery<IReadOnlyList<ScopeCatalogReadModel>>;
