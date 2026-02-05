using Application.Abstractions.Messaging;
using Application.ReadModels.MasterData;

namespace Application.Features.Warehouses.GetWarehouses;

public sealed record GetWarehousesQuery(
    int Page = 1,
    int PageSize = 50,
    string? Search = null,
    Guid? BranchId = null
) : IQuery<IReadOnlyList<WarehouseReadModel>>
{
    public int Take => PageSize;
    public int Skip => (Page - 1) * PageSize;
}
