using SharedKernel;

namespace Domain.Suppliers;

public sealed class Supplier : Entity
{
    public Guid Id { get; set; }
    public string IdentificationType { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = string.Empty;
    public string MainCurrency { get; set; } = string.Empty;
    public decimal? CreditLimit { get; private set; }
    public int? CreditDays { get; private set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<Domain.SupplierContacts.SupplierContact> Contacts { get; set; } =
        new List<Domain.SupplierContacts.SupplierContact>();
    public ICollection<Domain.SupplierAttachments.SupplierAttachment> Attachments { get; set; } =
        new List<Domain.SupplierAttachments.SupplierAttachment>();

    public ICollection<Domain.PurchaseOrders.PurchaseOrder> PurchaseOrders { get; set; } =
        new List<Domain.PurchaseOrders.PurchaseOrder>();
    public ICollection<Domain.AccountsPayable.AccountPayable> AccountsPayables { get; set; } =
        new List<Domain.AccountsPayable.AccountPayable>();

    // =========================
    // Domain events
    // =========================

    public void RaiseCreated() =>
        Raise(
            new SupplierCreatedDomainEvent(
                Id,
                IdentificationType,
                Identification,
                LegalName,
                TradeName,
                Email,
                Phone,
                PaymentTerms,
                MainCurrency,
                IsActive,
                CreatedAt,
                UpdatedAt
            )
        );

    public void RaiseUpdated() =>
        Raise(
            new SupplierUpdatedDomainEvent(
                Id,
                LegalName,
                TradeName,
                Email,
                Phone,
                PaymentTerms,
                MainCurrency,
                IsActive,
                UpdatedAt
            )
        );

    public void RaiseDeleted() => Raise(new SupplierDeletedDomainEvent(Id));

    // =========================
    // Domain behavior (root)
    // =========================

    public void UpdateBasicInfo(
        string legalName,
        string tradeName,
        string email,
        string phone,
        string? paymentTerms,
        string? mainCurrency,
        bool isActive
    )
    {
        TradeName = tradeName;
        LegalName = legalName;
        Email = email;
        Phone = phone;
        PaymentTerms = paymentTerms!;
        MainCurrency = mainCurrency!;
        IsActive = isActive;

        UpdatedAt = DateTime.UtcNow;

        RaiseUpdated();
    }

    public void SetCreditConditions(decimal creditLimit, int creditDays)
    {
        if (creditLimit <= 0)
            throw new InvalidOperationException("Credit limit must be greater than zero.");

        if (creditDays <= 0)
            throw new InvalidOperationException("Credit days must be greater than zero.");

        CreditLimit = creditLimit;
        CreditDays = creditDays;
        UpdatedAt = DateTime.UtcNow;

        Raise(new SupplierCreditConditionsSetDomainEvent(Id, creditLimit, creditDays, UpdatedAt));
    }
}
