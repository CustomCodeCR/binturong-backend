using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Purchases.Requests.GetPurchaseRequestsSelect;

public sealed record GetPurchaseRequestsSelectQuery(string? Search = null)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
