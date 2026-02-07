using Application.Abstractions.Messaging;

namespace Application.Features.Quotes.GetQuotes;

public sealed record GetQuotesQuery(int Page, int PageSize, string? Search)
    : IQuery<IReadOnlyList<Application.ReadModels.Sales.QuoteReadModel>>
{
    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}
