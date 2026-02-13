using Application.Abstractions.Messaging;

namespace Application.Features.Invoices.ConvertFromQuote;

public sealed record ConvertQuoteToInvoiceCommand(
    Guid QuoteId,
    DateTime IssueDate,
    string DocumentType
) : ICommand<Guid>;
