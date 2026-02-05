namespace Application.ReadModels.Inventory;

public sealed class BranchInventoryItemReadModel
{
    public Guid ProductId { get; init; }
    public string? ProductName { get; init; }
    public decimal Stock { get; init; }
}
