using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Products.GetProductsSelect;

public sealed record GetProductsSelectQuery(string? Search = null, bool OnlyActive = true)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
