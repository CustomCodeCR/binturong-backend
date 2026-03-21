namespace Application.ReadModels.Discounts;

public sealed class DiscountPolicyReadModel
{
    public string Id { get; init; } = default!; // "discount_policy:{PolicyId}"
    public Guid PolicyId { get; init; }

    public string Name { get; init; } = default!;
    public decimal MaxDiscountPercentage { get; init; }
    public bool RequiresApprovalAboveLimit { get; init; }
    public bool IsActive { get; init; }
}
