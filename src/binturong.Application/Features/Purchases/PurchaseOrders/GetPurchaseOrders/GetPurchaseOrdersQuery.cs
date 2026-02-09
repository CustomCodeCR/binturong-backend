using Application.Abstractions.Messaging;
using Application.ReadModels.Purchases;

namespace Application.Features.Purchases.PurchaseOrders.GetPurchaseOrders;

public sealed record GetPurchaseOrdersQuery(int Page, int PageSize, string? Search)
    : IQuery<IReadOnlyList<PurchaseOrderReadModel>>
{
    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}
