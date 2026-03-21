using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Discounts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Discounts.RequestLineApproval;

internal sealed class RequestLineDiscountApprovalCommandHandler
    : ICommandHandler<RequestLineDiscountApprovalCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RequestLineDiscountApprovalCommandHandler(
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

    public async Task<Result<Guid>> Handle(
        RequestLineDiscountApprovalCommand cmd,
        CancellationToken ct
    )
    {
        var so = await _db
            .SalesOrders.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == cmd.SalesOrderId, ct);

        if (so is null)
            return Result.Failure<Guid>(
                Error.NotFound(
                    "SalesOrders.NotFound",
                    $"Sales order '{cmd.SalesOrderId}' not found."
                )
            );

        if (cmd.DiscountPerc < 0 || cmd.DiscountPerc > 100)
            return Result.Failure<Guid>(DiscountErrors.PercentageInvalid);

        if (string.IsNullOrWhiteSpace(cmd.Reason))
            return Result.Failure<Guid>(DiscountErrors.ReasonRequired);

        var req = so.CreateApprovalRequestForLineDiscount(
            cmd.SalesOrderDetailId,
            cmd.DiscountPerc,
            _currentUser.UserId,
            cmd.Reason,
            DateTime.UtcNow
        );

        _db.DiscountApprovalRequests.Add(req);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Discounts",
            "DiscountApprovalRequest",
            req.Id,
            "DISCOUNT_LINE_APPROVAL_REQUESTED",
            string.Empty,
            $"salesOrderId={req.SalesOrderId}; detailId={req.SalesOrderDetailId}; perc={req.RequestedPercentage}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(req.Id);
    }
}
