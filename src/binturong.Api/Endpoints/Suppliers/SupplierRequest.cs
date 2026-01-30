namespace Api.Endpoints.Suppliers;

public sealed record CreateSupplierRequest(
    string IdentificationType,
    string Identification,
    string LegalName,
    string TradeName,
    string Email,
    string Phone,
    string PaymentTerms,
    string MainCurrency,
    bool IsActive = true
);

public sealed record UpdateSupplierRequest(
    string LegalName,
    string TradeName,
    string Email,
    string Phone,
    string? PaymentTerms,
    string? MainCurrency,
    bool IsActive
);
