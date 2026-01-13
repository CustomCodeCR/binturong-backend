namespace Application.ReadModels.MasterData;

public sealed class WarehouseReadModel
{
    public string Id { get; init; } = default!; // "warehouse:{WarehouseId}"
    public int WarehouseId { get; init; }

    public int BranchId { get; init; }
    public string BranchCode { get; init; } = default!;
    public string BranchName { get; init; } = default!;

    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; }

    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
