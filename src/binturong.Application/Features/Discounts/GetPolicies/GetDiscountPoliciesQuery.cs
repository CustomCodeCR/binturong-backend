using Application.Abstractions.Messaging;
using Application.ReadModels.Discounts;

namespace Application.Features.Discounts.GetPolicies;

public sealed record GetDiscountPoliciesQuery(int Page, int PageSize, string? Search)
    : IQuery<IReadOnlyList<DiscountPolicyReadModel>>;
