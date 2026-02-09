using Application.Abstractions.Messaging;
using Application.ReadModels.Purchases;

namespace Application.Features.Purchases.PurchaseReceipts.GetPurchaseReceipts;

public sealed record GetPurchaseReceiptsQuery(int Page, int PageSize, string? Search)
    : IQuery<IReadOnlyList<PurchaseReceiptReadModel>>
{
    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}
