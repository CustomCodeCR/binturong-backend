using Application.Abstractions.Messaging;
using Application.ReadModels.MasterData;

namespace Application.Features.Branches.GetBranches;

public sealed record GetBranchesQuery(int Page = 1, int PageSize = 50, string? Search = null)
    : IQuery<IReadOnlyList<BranchReadModel>>
{
    public int Take => PageSize;
    public int Skip => (Page - 1) * PageSize;
}
