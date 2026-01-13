namespace Application.ReadModels.MasterData;

public sealed class TaxReadModel
{
    public string Id { get; init; } = default!; // "tax:{TaxId}"
    public int TaxId { get; init; }

    public string Name { get; init; } = default!;
    public string Code { get; init; } = default!;
    public decimal Percentage { get; init; }
    public bool IsActive { get; init; }
}
