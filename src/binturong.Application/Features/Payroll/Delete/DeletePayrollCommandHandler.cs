using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Payroll.Delete;

internal sealed class DeletePayrollCommandHandler : ICommandHandler<DeletePayrollCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _rt;

    public DeletePayrollCommandHandler(
        IApplicationDbContext db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser,
        IRealtimeNotifier rt
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
        _rt = rt;
    }

    public async Task<Result> Handle(DeletePayrollCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var payroll = await _db
            .Payrolls.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == cmd.PayrollId, ct);

        if (payroll is null)
            return Result.Failure(PayrollErrors.NotFound(cmd.PayrollId));

        var before =
            $"periodCode={payroll.PeriodCode}; start={payroll.StartDate}; end={payroll.EndDate}; type={payroll.PayrollType}; status={payroll.Status}; details={payroll.Details.Count}";

        if (payroll.Details.Count > 0)
            _db.PayrollDetails.RemoveRange(payroll.Details);

        _db.Payrolls.Remove(payroll);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Payroll",
            "Payroll",
            cmd.PayrollId,
            "PAYROLL_DELETED",
            before,
            string.Empty,
            ip,
            ua,
            ct
        );

        await _rt.NotifyUserAsync(userId, "payroll.deleted", new { payrollId = cmd.PayrollId }, ct);

        return Result.Success();
    }
}
