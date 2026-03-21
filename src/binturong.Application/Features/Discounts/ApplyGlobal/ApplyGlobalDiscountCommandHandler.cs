using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Discounts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Discounts.ApplyGlobal;

internal sealed class ApplyGlobalDiscountCommandHandler
    : ICommandHandler<ApplyGlobalDiscountCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public ApplyGlobalDiscountCommandHandler(
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

    public async Task<Result> Handle(ApplyGlobalDiscountCommand cmd, CancellationToken ct)
    {
        var so = await _db
            .SalesOrders.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == cmd.SalesOrderId, ct);

        if (so is null)
            return Result.Failure(
                Error.NotFound(
                    "SalesOrders.NotFound",
                    $"Sales order '{cmd.SalesOrderId}' not found."
                )
            );

        var policy = await _db.DiscountPolicies.FirstOrDefaultAsync(x => x.Id == cmd.PolicyId, ct);
        if (policy is null)
            return Result.Failure(DiscountErrors.PolicyNotFound(cmd.PolicyId));

        var result = so.ApplyGlobalDiscount(
            cmd.DiscountPerc,
            cmd.Reason,
            _currentUser.UserId,
            DateTime.UtcNow,
            policy
        );

        if (result.IsFailure)
            return result;

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Discounts",
            "SalesOrder",
            so.Id,
            "SALES_ORDER_GLOBAL_DISCOUNT_APPLIED",
            string.Empty,
            $"perc={cmd.DiscountPerc}; policyId={cmd.PolicyId}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
