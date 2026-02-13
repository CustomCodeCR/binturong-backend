using Application.Abstractions.Messaging;

namespace Application.Features.Contracts.ConvertFromQuote;

public sealed record ConvertQuoteToContractCommand(
    Guid QuoteId,
    DateOnly StartDate,
    DateOnly? EndDate,
    Guid ResponsibleUserId,
    string Description,
    string Notes,
    bool AutoRenewEnabled,
    int AutoRenewEveryDays,
    int ExpiryNoticeDays
) : ICommand<Guid>;
