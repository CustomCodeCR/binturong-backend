using Application.Abstractions.Messaging;

namespace Application.Features.Quotes.Create;

public sealed record CreateQuoteCommand(
    string Code,
    Guid ClientId,
    Guid? BranchId,
    DateTime IssueDate,
    DateTime ValidUntil,
    string Currency,
    decimal ExchangeRate,
    string? Notes
) : ICommand<Guid>;
