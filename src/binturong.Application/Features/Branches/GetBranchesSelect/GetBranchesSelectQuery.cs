using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Branches.GetBranchesSelect;

public sealed record GetBranchesSelectQuery(string? Search = null, bool OnlyActive = true)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
