using Application.Abstractions.Messaging;
using Application.ReadModels.Sales;

namespace Application.Features.Contracts.GetContracts;

public sealed record GetContractsQuery(int Page, int PageSize, string? Search)
    : IQuery<IReadOnlyList<ContractReadModel>>
{
    public int Skip => (Page <= 1 ? 0 : (Page - 1) * Take);
    public int Take => PageSize <= 0 ? 50 : PageSize;
}
