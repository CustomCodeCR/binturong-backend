using Application.Abstractions.Messaging;
using Application.ReadModels.MasterData;

namespace Application.Features.Taxes.GetTaxes;

public sealed record GetTaxesQuery(int Page = 1, int PageSize = 50, string? Search = null)
    : IQuery<IReadOnlyList<TaxReadModel>>
{
    public int Take => PageSize;
    public int Skip => (Page - 1) * PageSize;
}
