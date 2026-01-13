namespace Application.ReadModels.Inventory;

public sealed class ProductStockReadModel
{
    public string Id { get; init; } = default!; // "stock:{ProductId}"
    public int ProductId { get; init; }

    public string ProductName { get; init; } = default!;
    public decimal TotalStock { get; init; }

    public IReadOnlyList<WarehouseStockReadModel> Warehouses { get; init; } = [];

    public DateTime UpdatedAt { get; init; }
}

public sealed class WarehouseStockReadModel
{
    public int WarehouseId { get; init; }
    public string WarehouseCode { get; init; } = default!;
    public string WarehouseName { get; init; } = default!;

    public decimal CurrentStock { get; init; }
    public decimal MinStock { get; init; }
    public decimal MaxStock { get; init; }
}
