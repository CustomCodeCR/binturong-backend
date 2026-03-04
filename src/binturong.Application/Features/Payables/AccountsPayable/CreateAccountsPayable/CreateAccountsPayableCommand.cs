using Application.Abstractions.Messaging;

namespace Application.Features.Payables.AccountsPayable.CreateAccountsPayable;

public sealed record CreateAccountsPayableCommand(
    Guid SupplierId,
    Guid? PurchaseOrderId,
    string SupplierInvoiceId,
    DateOnly DocumentDate,
    DateOnly DueDate,
    decimal TotalAmount,
    string Currency
) : ICommand<Guid>;
