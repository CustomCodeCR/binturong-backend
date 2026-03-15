using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.ProductCategories.GetProductCategoriesSelect;

public sealed record GetProductCategoriesSelectQuery(string? Search = null, bool OnlyActive = true)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
