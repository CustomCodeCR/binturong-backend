namespace Application.ReadModels.MasterData;

public sealed class UnitOfMeasureReadModel
{
    public string Id { get; init; } = default!; // "uom:{UomId}"
    public Guid UomId { get; init; }

    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
