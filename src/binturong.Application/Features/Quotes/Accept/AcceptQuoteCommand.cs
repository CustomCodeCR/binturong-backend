using Application.Abstractions.Messaging;

namespace Application.Features.Quotes.Accept;

public sealed record AcceptQuoteCommand(Guid QuoteId) : ICommand;
