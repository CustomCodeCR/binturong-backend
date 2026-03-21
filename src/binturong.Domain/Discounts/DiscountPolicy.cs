using SharedKernel;

namespace Domain.Discounts;

public sealed class DiscountPolicy : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Example: seller can apply up to 10%
    public decimal MaxDiscountPercentage { get; set; }

    // If true, discounts above limit create approval request instead of being directly applied
    public bool RequiresApprovalAboveLimit { get; set; }

    public bool IsActive { get; set; }

    public void RaiseCreated() =>
        Raise(
            new DiscountPolicyCreatedDomainEvent(
                Id,
                Name,
                MaxDiscountPercentage,
                RequiresApprovalAboveLimit,
                IsActive
            )
        );

    public void RaiseUpdated() =>
        Raise(
            new DiscountPolicyUpdatedDomainEvent(
                Id,
                Name,
                MaxDiscountPercentage,
                RequiresApprovalAboveLimit,
                IsActive,
                DateTime.UtcNow
            )
        );

    public void RaiseDeleted() => Raise(new DiscountPolicyDeletedDomainEvent(Id));

    public Result ValidatePercentage(decimal percentage)
    {
        if (!IsActive)
            return Result.Failure(DiscountErrors.PolicyInactive);

        if (percentage < 0 || percentage > 100)
            return Result.Failure(DiscountErrors.PercentageInvalid);

        if (percentage > MaxDiscountPercentage)
            return Result.Failure(DiscountErrors.DiscountExceedsPolicy);

        return Result.Success();
    }

    public Result ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return Result.Failure(
                Error.Validation("Discounts.PolicyNameRequired", "Policy name is required.")
            );
        }

        if (MaxDiscountPercentage < 0 || MaxDiscountPercentage > 100)
            return Result.Failure(DiscountErrors.MaxPercentageInvalid);

        return Result.Success();
    }
}
