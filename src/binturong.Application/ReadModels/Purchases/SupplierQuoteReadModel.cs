namespace Application.ReadModels.Purchases;

public sealed class SupplierQuoteReadModel
{
    public string Id { get; set; } = default!; // "supplier_quote:{SupplierQuoteId}"
    public Guid SupplierQuoteId { get; set; }

    public string Code { get; set; } = default!;
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = default!;

    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }

    public DateTime RequestedAtUtc { get; set; }
    public DateTime? RespondedAtUtc { get; set; }

    public string Status { get; set; } = default!; // Sent | Responded | Rejected
    public string? Notes { get; set; }
    public string? SupplierMessage { get; set; }
    public string? RejectReason { get; set; }

    // IMPORTANT: use mutable List + set; so Mongo updates ($push) are reliable
    public List<SupplierQuoteLineReadModel> Lines { get; set; } = new();
    public List<SupplierQuoteResponseLineReadModel> ResponseLines { get; set; } = new();
}

public sealed class SupplierQuoteLineReadModel
{
    public Guid SupplierQuoteLineId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal Quantity { get; set; }
}

public sealed class SupplierQuoteResponseLineReadModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public decimal DiscountPerc { get; set; }
    public decimal TaxPerc { get; set; }
    public string? Conditions { get; set; }
}
