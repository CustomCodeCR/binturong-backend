namespace Application.ReadModels.Sales;

public sealed class InvoiceReadModel
{
    public string Id { get; init; } = default!; // "invoice:{InvoiceId}"
    public int InvoiceId { get; init; }

    public string? TaxKey { get; init; }
    public string? Consecutive { get; init; }

    public int ClientId { get; init; }
    public string ClientName { get; init; } = default!;

    public int? BranchId { get; init; }
    public string? BranchName { get; init; }

    public int? SalesOrderId { get; init; }
    public int? ContractId { get; init; }

    public DateTime IssueDate { get; init; }
    public string DocumentType { get; init; } = default!;

    public string Currency { get; init; } = default!;
    public decimal ExchangeRate { get; init; }

    public decimal Subtotal { get; init; }
    public decimal Taxes { get; init; }
    public decimal Discounts { get; init; }
    public decimal Total { get; init; }

    public string TaxStatus { get; init; } = default!;
    public string InternalStatus { get; init; } = default!;
    public bool EmailSent { get; init; }

    public string? PdfS3Key { get; init; }
    public string? XmlS3Key { get; init; }

    public IReadOnlyList<InvoiceLineReadModel> Lines { get; init; } = [];

    // Payment summary for CRM/ERP screens
    public decimal PaidAmount { get; init; }
    public decimal PendingAmount { get; init; }
}

public sealed class InvoiceLineReadModel
{
    public int InvoiceDetailId { get; init; }

    public int ProductId { get; init; }
    public string Description { get; init; } = default!;

    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountPerc { get; init; }
    public decimal TaxPerc { get; init; }
    public decimal LineTotal { get; init; }
}
