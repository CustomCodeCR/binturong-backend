namespace Application.ReadModels.Sales;

public sealed class QuoteReadModel
{
    public string Id { get; init; } = default!; // "quote:{QuoteId}"
    public Guid QuoteId { get; init; }
    public string Code { get; init; } = default!;

    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = default!;

    public Guid? BranchId { get; init; }
    public string? BranchName { get; init; }

    public DateTime IssueDate { get; init; }
    public DateTime? ValidUntil { get; init; }
    public string Status { get; init; } = default!;

    public string Currency { get; init; } = default!;
    public decimal ExchangeRate { get; init; }

    public decimal Subtotal { get; init; }
    public decimal Taxes { get; init; }
    public decimal Discounts { get; init; }
    public decimal Total { get; init; }

    public bool AcceptedByClient { get; init; }
    public DateTime? AcceptanceDate { get; init; }
    public int Version { get; init; }
    public string? Notes { get; init; }

    public IReadOnlyList<QuoteLineReadModel> Lines { get; init; } = [];
}

public sealed class QuoteLineReadModel
{
    public Guid QuoteDetailId { get; init; }

    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;

    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountPerc { get; init; }
    public decimal TaxPerc { get; init; }
    public decimal LineTotal { get; init; }
}
