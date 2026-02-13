using Application.Abstractions.Messaging;

namespace Application.Features.Invoices.Update;

public sealed record UpdateInvoiceFromApiCommand(
    Guid InvoiceId,
    DateTime IssueDate,
    string DocumentType,
    string Currency,
    decimal ExchangeRate,
    string? Notes,
    string InternalStatus
) : ICommand;
