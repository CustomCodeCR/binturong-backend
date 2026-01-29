using Application.Abstractions.Messaging;
using Application.ReadModels.MasterData;

namespace Application.Features.UnitsOfMeasure.GetUnitsOfMeasure;

public sealed record GetUnitsOfMeasureQuery(int Page = 1, int PageSize = 50, string? Search = null)
    : IQuery<IReadOnlyList<UnitOfMeasureReadModel>>
{
    public int Take => PageSize;
    public int Skip => (Page - 1) * PageSize;
}
