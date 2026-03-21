using Application.Abstractions.Messaging;
using Application.ReadModels.Discounts;

namespace Application.Features.Discounts.GetDiscountHistory;

public sealed record GetDiscountHistoryQuery(
    int Page,
    int PageSize,
    string? Search,
    Guid? UserId,
    DateTime? FromUtc,
    DateTime? ToUtc
) : IQuery<IReadOnlyList<DiscountHistoryReadModel>>;
