using SharedKernel;

namespace Domain.Suppliers;

public sealed record SupplierCreatedDomainEvent(
    Guid SupplierId,
    string IdentificationType,
    string Identification,
    string LegalName,
    string TradeName,
    string Email,
    string Phone,
    string PaymentTerms,
    string MainCurrency,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record SupplierUpdatedDomainEvent(
    Guid SupplierId,
    string LegalName,
    string TradeName,
    string Email,
    string Phone,
    string PaymentTerms,
    string MainCurrency,
    bool IsActive,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record SupplierDeletedDomainEvent(Guid SupplierId) : IDomainEvent;

public sealed record SupplierCreditConditionsSetDomainEvent(
    Guid SupplierId,
    decimal CreditLimit,
    int CreditDays,
    DateTime UpdatedAt
) : IDomainEvent;
