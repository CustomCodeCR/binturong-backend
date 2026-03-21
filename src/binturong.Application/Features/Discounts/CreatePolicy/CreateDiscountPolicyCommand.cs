using Application.Abstractions.Messaging;

namespace Application.Features.Discounts.CreatePolicy;

public sealed record CreateDiscountPolicyCommand(
    string Name,
    decimal MaxDiscountPercentage,
    bool RequiresApprovalAboveLimit,
    bool IsActive
) : ICommand<Guid>;
