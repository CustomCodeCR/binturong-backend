using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.UnitsOfMeasure.GetUnitsOfMeasureSelect;

public sealed record GetUnitsOfMeasureSelectQuery(string? Search = null, bool OnlyActive = true)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
