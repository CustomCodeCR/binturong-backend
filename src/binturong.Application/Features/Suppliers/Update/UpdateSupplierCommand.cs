using Application.Abstractions.Messaging;

namespace Application.Features.Suppliers.Update;

public sealed record UpdateSupplierCommand(
    Guid SupplierId,
    string LegalName,
    string TradeName,
    string Email,
    string Phone,
    string? PaymentTerms,
    string? MainCurrency,
    bool IsActive
) : ICommand;
