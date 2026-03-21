namespace Application.ReadModels.Reports;

public sealed class ClientReportReadModel
{
    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = default!;

    public bool HasData { get; init; }
    public string? Message { get; init; }

    public IReadOnlyList<ClientPurchaseReportItemReadModel> Purchases { get; init; } = [];
    public IReadOnlyList<ClientServiceReportItemReadModel> Services { get; init; } = [];
    public IReadOnlyList<ClientInvoiceReportItemReadModel> Invoices { get; init; } = [];
}

public sealed class ClientPurchaseReportItemReadModel
{
    public Guid SalesOrderId { get; init; }
    public string Code { get; init; } = default!;
    public DateTime OrderDate { get; init; }
    public decimal Total { get; init; }
    public string Status { get; init; } = default!;
}

public sealed class ClientServiceReportItemReadModel
{
    public Guid ServiceOrderId { get; init; }
    public string Code { get; init; } = default!;
    public DateTime ScheduledDate { get; init; }
    public string Status { get; init; } = default!;
    public string? ContractCode { get; init; }
}

public sealed class ClientInvoiceReportItemReadModel
{
    public Guid InvoiceId { get; init; }
    public string? Consecutive { get; init; }
    public DateTime IssueDate { get; init; }
    public decimal Total { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal PendingAmount { get; init; }
    public string TaxStatus { get; init; } = default!;
}
