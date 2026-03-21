namespace Application.ReadModels.Reports;

public sealed class InventoryReportReadModel
{
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }

    public bool HasData { get; init; }
    public string? Message { get; init; }

    public IReadOnlyList<InventoryReportItemReadModel> Items { get; init; } = [];
}

public sealed class InventoryReportItemReadModel
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;

    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }

    public decimal TotalStock { get; init; }
    public DateTime UpdatedAt { get; init; }
}
