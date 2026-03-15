using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Warehouses.GetWarehousesSelect;

public sealed record GetWarehousesSelectQuery(string? Search = null, bool OnlyActive = true)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
