using Application.Abstractions.Messaging;
using Application.ReadModels.Sales;

namespace Application.Features.Quotes.GetQuoteById;

public sealed record GetQuoteByIdQuery(Guid QuoteId) : IQuery<QuoteReadModel>;
