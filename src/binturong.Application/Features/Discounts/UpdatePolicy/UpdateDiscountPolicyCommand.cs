using Application.Abstractions.Messaging;

namespace Application.Features.Discounts.UpdatePolicy;

public sealed record UpdateDiscountPolicyCommand(
    Guid PolicyId,
    string Name,
    decimal MaxDiscountPercentage,
    bool RequiresApprovalAboveLimit,
    bool IsActive
) : ICommand;
