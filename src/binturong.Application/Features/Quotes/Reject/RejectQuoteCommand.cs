using Application.Abstractions.Messaging;

namespace Application.Features.Quotes.Reject;

public sealed record RejectQuoteCommand(Guid QuoteId, string Reason) : ICommand;
