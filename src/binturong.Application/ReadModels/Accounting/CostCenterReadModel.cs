namespace Application.ReadModels.Accounting;

public sealed class CostCenterReadModel
{
    public string Id { get; init; } = default!; // "cc:{CostCenterId}"
    public int CostCenterId { get; init; }

    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public bool IsActive { get; init; }
}
