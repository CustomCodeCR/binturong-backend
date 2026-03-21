using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Discounts.GetPoliciesSelect;

public sealed record GetDiscountPoliciesSelectQuery(string? Search)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
