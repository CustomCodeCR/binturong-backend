using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Discounts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Discounts.ApplyLineApproved;

internal sealed class ApplyApprovedLineDiscountCommandHandler
    : ICommandHandler<ApplyApprovedLineDiscountCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public ApplyApprovedLineDiscountCommandHandler(
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

    public async Task<Result> Handle(ApplyApprovedLineDiscountCommand cmd, CancellationToken ct)
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

        var approval = await _db.DiscountApprovalRequests.FirstOrDefaultAsync(
            x => x.Id == cmd.ApprovalRequestId,
            ct
        );
        if (approval is null)
            return Result.Failure(DiscountErrors.ApprovalRequestNotFound(cmd.ApprovalRequestId));

        var result = so.ApplyLineDiscountApproved(
            cmd.SalesOrderDetailId,
            cmd.DiscountPerc,
            cmd.Reason,
            _currentUser.UserId,
            DateTime.UtcNow,
            approval
        );

        if (result.IsFailure)
            return result;

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Discounts",
            "SalesOrder",
            so.Id,
            "SALES_ORDER_LINE_DISCOUNT_APPLIED_APPROVED",
            string.Empty,
            $"detailId={cmd.SalesOrderDetailId}; approvalRequestId={cmd.ApprovalRequestId}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
