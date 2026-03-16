using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Purchases.Orders.GetPurchaseOrdersSelect;

public sealed record GetPurchaseOrdersSelectQuery(string? Search = null)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
