namespace Application.ReadModels.Purchases;

public sealed class SupplierQuoteReadModel
{
    public string Id { get; init; } = default!; // "supplier_quote:{SupplierQuoteId}"
    public Guid SupplierQuoteId { get; init; }

    public string Code { get; init; } = default!;
    public Guid SupplierId { get; init; }
    public string SupplierName { get; init; } = default!;

    public Guid? BranchId { get; init; }
    public string? BranchName { get; init; }

    public DateTime RequestedAtUtc { get; init; }
    public DateTime? RespondedAtUtc { get; init; }

    public string Status { get; init; } = default!; // Sent | Responded | Rejected
    public string? Notes { get; init; }
    public string? SupplierMessage { get; init; }
    public string? RejectReason { get; init; }

    public IReadOnlyList<SupplierQuoteLineReadModel> Lines { get; init; } = [];
    public IReadOnlyList<SupplierQuoteResponseLineReadModel> ResponseLines { get; init; } = [];
}

public sealed class SupplierQuoteLineReadModel
{
    public Guid SupplierQuoteLineId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public decimal Quantity { get; init; }
}

public sealed class SupplierQuoteResponseLineReadModel
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public decimal UnitPrice { get; init; }
    public decimal DiscountPerc { get; init; }
    public decimal TaxPerc { get; init; }
    public string? Conditions { get; init; }
}
