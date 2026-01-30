using Application.Abstractions.Messaging;

namespace Application.Features.Suppliers.Create;

public sealed record CreateSupplierCommand(
    string IdentificationType,
    string Identification,
    string LegalName,
    string TradeName,
    string Email,
    string Phone,
    string PaymentTerms,
    string MainCurrency,
    bool IsActive = true
) : ICommand<Guid>;
