using Application.Abstractions.Messaging;

namespace Application.Features.Invoices.Create;

public sealed record CreateInvoiceLine(
    Guid ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPerc,
    decimal TaxPerc,
    decimal LineTotal
);

public sealed record CreateInvoiceCommand(
    Guid ClientId,
    Guid? BranchId,
    Guid? SalesOrderId,
    Guid? ContractId,
    DateTime IssueDate,
    string DocumentType,
    string Currency,
    decimal ExchangeRate,
    decimal Subtotal,
    decimal Taxes,
    decimal Discounts,
    decimal Total,
    IReadOnlyList<CreateInvoiceLine> Lines
) : ICommand<Guid>;
