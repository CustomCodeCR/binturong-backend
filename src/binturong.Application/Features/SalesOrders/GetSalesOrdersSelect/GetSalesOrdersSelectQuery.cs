using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.SalesOrders.GetSalesOrdersSelect;

public sealed record GetSalesOrdersSelectQuery(string? Search = null)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
