namespace Application.ReadModels.MasterData;

public sealed class ProductCategoryReadModel
{
    public string Id { get; init; } = default!; // "category:{CategoryId}"
    public int CategoryId { get; init; }

    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}
