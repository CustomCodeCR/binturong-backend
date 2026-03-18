using Application.Abstractions.Messaging;
using Application.Common.Selects;

namespace Application.Features.Quotes.GetQuotesSelect;

public sealed record GetQuotesSelectQuery(string? Search = null)
    : IQuery<IReadOnlyList<SelectOptionDto>>;
