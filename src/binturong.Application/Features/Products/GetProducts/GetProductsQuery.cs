using Application.Abstractions.Messaging;
using Application.ReadModels.Inventory;

namespace Application.Features.Products.GetProducts;

public sealed record GetProductsQuery(int Page = 1, int PageSize = 50, string? Search = null)
    : IQuery<IReadOnlyList<ProductReadModel>>
{
    public int Take => PageSize;
    public int Skip => (Page - 1) * PageSize;
}
