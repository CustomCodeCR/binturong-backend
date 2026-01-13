namespace Application.ReadModels.Inventory;

public sealed class ProductReadModel
{
    public string Id { get; init; } = default!; // "product:{ProductId}"
    public int ProductId { get; init; }

    public string SKU { get; init; } = default!;
    public string? Barcode { get; init; }

    public string Name { get; init; } = default!;
    public string? Description { get; init; }

    public int? CategoryId { get; init; }
    public string? CategoryName { get; init; }

    public int? UomId { get; init; }
    public string? UomCode { get; init; }
    public string? UomName { get; init; }

    public int? TaxId { get; init; }
    public string? TaxCode { get; init; }
    public decimal TaxPercentage { get; init; }

    public decimal BasePrice { get; init; }
    public decimal AverageCost { get; init; }

    public bool IsService { get; init; }
    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    // E-commerce friendly flags (si luego lo agregás)
    public bool IsPublished { get; init; }
    public IReadOnlyList<string> ImageS3Keys { get; init; } = [];
}
