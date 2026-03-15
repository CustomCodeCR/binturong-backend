using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Taxes.GetTaxesSelect;

public sealed record GetTaxesSelectQuery(string? Search = null, bool OnlyActive = true)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
