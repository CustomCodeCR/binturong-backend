using Application.Abstractions.Messaging;
using Application.ReadModels.Discounts;

namespace Application.Features.Discounts.GetApprovalRequests;

public sealed record GetDiscountApprovalRequestsQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Status
) : IQuery<IReadOnlyList<DiscountApprovalRequestReadModel>>;
