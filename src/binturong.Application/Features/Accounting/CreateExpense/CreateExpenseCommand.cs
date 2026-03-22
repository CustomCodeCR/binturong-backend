using Application.Abstractions.Messaging;

namespace Application.Features.Accounting.CreateExpense;

public sealed record CreateExpenseCommand(
    decimal Amount,
    string Detail,
    string Category,
    DateTime EntryDateUtc,
    Guid SupplierId,
    string? ReceiptFileS3Key
) : ICommand<Guid>;
