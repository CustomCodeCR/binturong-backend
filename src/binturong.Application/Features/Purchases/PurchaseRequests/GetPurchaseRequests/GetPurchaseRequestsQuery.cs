using Application.Abstractions.Messaging;
using Application.ReadModels.Purchases;

namespace Application.Features.Purchases.PurchaseRequests.GetPurchaseRequests;

public sealed record GetPurchaseRequestsQuery(int Page, int PageSize, string? Search)
    : IQuery<IReadOnlyList<PurchaseRequestReadModel>>
{
    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}
