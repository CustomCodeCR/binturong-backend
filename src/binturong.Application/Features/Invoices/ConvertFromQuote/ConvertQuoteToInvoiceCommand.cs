using Application.Abstractions.Messaging;

namespace Application.Features.Invoices.ConvertFromQuote;

public sealed record ConvertQuoteToInvoiceCommand(
    Guid QuoteId,
    Guid? BranchId,
    DateTime IssueDate,
    string DocumentType,
    string Mode
) : ICommand<Guid>;
