using Application.Abstractions.Messaging;

namespace Application.Features.Accounting.CreateIncome;

public sealed record CreateIncomeCommand(
    decimal Amount,
    string Detail,
    string Category,
    DateTime EntryDateUtc,
    Guid ClientId,
    string InvoiceNumber
) : ICommand<Guid>;
