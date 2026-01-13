using SharedKernel;

namespace Domain.Clients;

public sealed class Client : Entity
{
    public Guid Id { get; set; }
    public string PersonType { get; set; } = string.Empty;
    public string IdentificationType { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PrimaryPhone { get; set; } = string.Empty;
    public string SecondaryPhone { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string ClientType { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<Domain.ClientAddresses.ClientAddress> Addresses { get; set; } =
        new List<Domain.ClientAddresses.ClientAddress>();
    public ICollection<Domain.ClientContacts.ClientContact> Contacts { get; set; } =
        new List<Domain.ClientContacts.ClientContact>();
    public ICollection<Domain.ClientAttachments.ClientAttachment> Attachments { get; set; } =
        new List<Domain.ClientAttachments.ClientAttachment>();

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
}
