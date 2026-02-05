using Application.Abstractions.Messaging;
using Application.ReadModels.MasterData;

namespace Application.Features.ProductCategories.GetProductCategories;

public sealed record GetProductCategoriesQuery(
    int Page = 1,
    int PageSize = 50,
    string? Search = null
) : IQuery<IReadOnlyList<ProductCategoryReadModel>>
{
    public int Take => PageSize;
    public int Skip => (Page - 1) * PageSize;
}
