namespace Application.ReadModels.MasterData;

public sealed class BranchReadModel
{
    public string Id { get; init; } = default!; // "branch:{BranchId}"
    public Guid BranchId { get; init; }

    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string Address { get; init; } = default!;
    public string Phone { get; init; } = default!;

    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public IReadOnlyList<BranchWarehouseSummaryReadModel> Warehouses { get; init; } = [];
}

public sealed class BranchWarehouseSummaryReadModel
{
    public int WarehouseId { get; init; }
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public bool IsActive { get; init; }
}
