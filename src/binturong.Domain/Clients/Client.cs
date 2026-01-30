using SharedKernel;

namespace Domain.Clients;

public sealed class Client : Entity
{
    public Guid Id { get; set; }

    public string PersonType { get; set; } = string.Empty; // NATURAL / LEGAL
    public string IdentificationType { get; set; } = string.Empty; // ID, PASSPORT, TAXID
    public string Identification { get; set; } = string.Empty;

    public string TradeName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string PrimaryPhone { get; set; } = string.Empty;
    public string? SecondaryPhone { get; set; }

    public string? Industry { get; set; }
    public string? ClientType { get; set; }

    public int Score { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // =========================
    // Navigation (children)
    // =========================

    public ICollection<Domain.ClientAddresses.ClientAddress> Addresses { get; set; } =
        new List<Domain.ClientAddresses.ClientAddress>();

    public ICollection<Domain.ClientContacts.ClientContact> Contacts { get; set; } =
        new List<Domain.ClientContacts.ClientContact>();

    public ICollection<Domain.ClientAttachments.ClientAttachment> Attachments { get; set; } =
        new List<Domain.ClientAttachments.ClientAttachment>();

    // Other relations (ERP/CRM)
    public ICollection<Domain.Quotes.Quote> Quotes { get; set; } = new List<Domain.Quotes.Quote>();
    public ICollection<Domain.SalesOrders.SalesOrder> SalesOrders { get; set; } =
        new List<Domain.SalesOrders.SalesOrder>();
    public ICollection<Domain.Contracts.Contract> Contracts { get; set; } =
        new List<Domain.Contracts.Contract>();
    public ICollection<Domain.Invoices.Invoice> Invoices { get; set; } =
        new List<Domain.Invoices.Invoice>();
    public ICollection<Domain.Payments.Payment> Payments { get; set; } =
        new List<Domain.Payments.Payment>();
    public ICollection<Domain.WebClients.WebClient> WebClients { get; set; } =
        new List<Domain.WebClients.WebClient>();
    public ICollection<Domain.ServiceOrders.ServiceOrder> ServiceOrders { get; set; } =
        new List<Domain.ServiceOrders.ServiceOrder>();

    public ICollection<Domain.MarketingAssetTracking.MarketingAssetTrackingEntry> MarketingTrackings { get; set; } =
        new List<Domain.MarketingAssetTracking.MarketingAssetTrackingEntry>();

    // =========================
    // Domain events
    // =========================

    public void RaiseCreated() =>
        Raise(
            new ClientCreatedDomainEvent(
                Id,
                PersonType,
                IdentificationType,
                Identification,
                TradeName,
                ContactName,
                Email,
                PrimaryPhone,
                SecondaryPhone,
                Industry,
                ClientType,
                Score,
                IsActive,
                CreatedAt,
                UpdatedAt
            )
        );

    public void RaiseUpdated() =>
        Raise(
            new ClientUpdatedDomainEvent(
                Id,
                TradeName,
                ContactName,
                Email,
                PrimaryPhone,
                SecondaryPhone,
                Industry,
                ClientType,
                Score,
                IsActive,
                UpdatedAt
            )
        );

    public void RaiseDeleted() => Raise(new ClientDeletedDomainEvent(Id));

    // =========================
    // Domain behavior (root)
    // =========================

    public void UpdateBasicInfo(
        string tradeName,
        string contactName,
        string email,
        string primaryPhone,
        string? secondaryPhone,
        string? industry,
        string? clientType,
        int score,
        bool isActive
    )
    {
        TradeName = tradeName;
        ContactName = contactName;
        Email = email;
        PrimaryPhone = primaryPhone;
        SecondaryPhone = secondaryPhone;
        Industry = industry;
        ClientType = clientType;
        Score = score;
        IsActive = isActive;

        UpdatedAt = DateTime.UtcNow;

        RaiseUpdated();
    }
}
