using Application.ReadModels.Common;

namespace Application.ReadModels.CRM;

public sealed class ClientReadModel
{
    public string Id { get; init; } = default!; // "client:{ClientId}"
    public Guid ClientId { get; init; }

    public string PersonType { get; init; } = default!;
    public string IdentificationType { get; init; } = default!;
    public string Identification { get; init; } = default!;

    public string TradeName { get; init; } = default!;
    public string ContactName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string PrimaryPhone { get; init; } = default!;
    public string? SecondaryPhone { get; init; }

    public string? Industry { get; init; }
    public string? ClientType { get; init; }
    public int Score { get; init; }

    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public IReadOnlyList<AddressReadModel> Addresses { get; init; } = [];
    public IReadOnlyList<ContactReadModel> Contacts { get; init; } = [];
    public IReadOnlyList<AttachmentReadModel> Attachments { get; init; } = [];

    // ERP/CRM KPIs (para pantallas)
    public ClientKpisReadModel Kpis { get; init; } = new();
}

public sealed class ClientKpisReadModel
{
    public decimal OutstandingBalance { get; init; } // invoices - payments
    public decimal TotalInvoicedLast90Days { get; init; }
    public int OpenQuotesCount { get; init; }
    public int OpenSalesOrdersCount { get; init; }
    public int ActiveContractsCount { get; init; }
    public int OpenServiceOrdersCount { get; init; }
}
