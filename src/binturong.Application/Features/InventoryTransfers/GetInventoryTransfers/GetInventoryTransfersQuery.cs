using Application.Abstractions.Messaging;
using Application.ReadModels.Inventory;

namespace Application.Features.InventoryTransfers.GetInventoryTransfers;

public sealed record GetInventoryTransfersQuery(
    int Page = 1,
    int PageSize = 50,
    string? Search = null
) : IQuery<IReadOnlyList<InventoryTransferReadModel>>
{
    public int Take => PageSize;
    public int Skip => (Page - 1) * PageSize;
}
