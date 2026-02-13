using Application.Abstractions.Messaging;

namespace Application.Features.Invoices.Update;

public sealed record UpdateInvoiceCommand(
    Guid InvoiceId,
    Guid ClientId,
    Guid? BranchId,
    DateTime IssueDate,
    string Currency,
    decimal ExchangeRate,
    decimal Subtotal,
    decimal Taxes,
    decimal Discounts,
    decimal Total
) : ICommand;
