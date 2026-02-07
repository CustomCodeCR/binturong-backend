using Application.Abstractions.Messaging;

namespace Application.Features.Quotes.Expire;

public sealed record ExpireQuoteCommand(Guid QuoteId) : ICommand;
