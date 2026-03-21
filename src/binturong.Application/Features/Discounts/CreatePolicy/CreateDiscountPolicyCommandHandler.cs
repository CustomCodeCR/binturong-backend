using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Discounts;
using SharedKernel;

namespace Application.Features.Discounts.CreatePolicy;

internal sealed class CreateDiscountPolicyCommandHandler
    : ICommandHandler<CreateDiscountPolicyCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateDiscountPolicyCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateDiscountPolicyCommand cmd, CancellationToken ct)
    {
        var policy = new DiscountPolicy
        {
            Id = Guid.NewGuid(),
            Name = cmd.Name?.Trim() ?? string.Empty,
            MaxDiscountPercentage = cmd.MaxDiscountPercentage,
            RequiresApprovalAboveLimit = cmd.RequiresApprovalAboveLimit,
            IsActive = cmd.IsActive,
        };

        var validation = policy.ValidateConfiguration();
        if (validation.IsFailure)
            return Result.Failure<Guid>(validation.Error);

        policy.RaiseCreated();

        _db.DiscountPolicies.Add(policy);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Discounts",
            "DiscountPolicy",
            policy.Id,
            "DISCOUNT_POLICY_CREATED",
            string.Empty,
            $"name={policy.Name}; max={policy.MaxDiscountPercentage}; requiresApproval={policy.RequiresApprovalAboveLimit}; active={policy.IsActive}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(policy.Id);
    }
}
