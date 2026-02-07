using Application.Abstractions.Messaging;

namespace Application.Features.Quotes.Send;

public sealed record SendQuoteCommand(Guid QuoteId) : ICommand;
