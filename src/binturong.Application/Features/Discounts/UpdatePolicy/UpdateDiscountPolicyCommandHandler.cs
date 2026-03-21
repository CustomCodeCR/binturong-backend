using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Discounts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Discounts.UpdatePolicy;

internal sealed class UpdateDiscountPolicyCommandHandler
    : ICommandHandler<UpdateDiscountPolicyCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateDiscountPolicyCommandHandler(
        IApplicationDbContext db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateDiscountPolicyCommand cmd, CancellationToken ct)
    {
        var policy = await _db.DiscountPolicies.FirstOrDefaultAsync(x => x.Id == cmd.PolicyId, ct);
        if (policy is null)
            return Result.Failure(DiscountErrors.PolicyNotFound(cmd.PolicyId));

        policy.Name = cmd.Name?.Trim() ?? string.Empty;
        policy.MaxDiscountPercentage = cmd.MaxDiscountPercentage;
        policy.RequiresApprovalAboveLimit = cmd.RequiresApprovalAboveLimit;
        policy.IsActive = cmd.IsActive;

        var validation = policy.ValidateConfiguration();
        if (validation.IsFailure)
            return validation;

        policy.RaiseUpdated();
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Discounts",
            "DiscountPolicy",
            policy.Id,
            "DISCOUNT_POLICY_UPDATED",
            string.Empty,
            $"name={policy.Name}; max={policy.MaxDiscountPercentage}; requiresApproval={policy.RequiresApprovalAboveLimit}; active={policy.IsActive}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
