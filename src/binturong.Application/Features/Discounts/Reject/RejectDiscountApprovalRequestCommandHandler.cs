using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Discounts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Discounts.Reject;

internal sealed class RejectDiscountApprovalRequestCommandHandler
    : ICommandHandler<RejectDiscountApprovalRequestCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RejectDiscountApprovalRequestCommandHandler(
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

    public async Task<Result> Handle(RejectDiscountApprovalRequestCommand cmd, CancellationToken ct)
    {
        var approval = await _db.DiscountApprovalRequests.FirstOrDefaultAsync(
            x => x.Id == cmd.ApprovalRequestId,
            ct
        );
        if (approval is null)
            return Result.Failure(DiscountErrors.ApprovalRequestNotFound(cmd.ApprovalRequestId));

        var result = approval.Reject(_currentUser.UserId, cmd.RejectionReason, DateTime.UtcNow);
        if (result.IsFailure)
            return result;

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Discounts",
            "DiscountApprovalRequest",
            approval.Id,
            "DISCOUNT_APPROVAL_REJECTED",
            string.Empty,
            $"salesOrderId={approval.SalesOrderId}; scope={approval.Scope}; reason={approval.RejectionReason}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success();
    }
}
