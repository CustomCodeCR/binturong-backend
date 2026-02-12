using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Payroll.Commission.Adjust;

internal sealed class AdjustPayrollCommissionCommandHandler
    : ICommandHandler<AdjustPayrollCommissionCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _rt;

    public AdjustPayrollCommissionCommandHandler(
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

    public async Task<Result> Handle(AdjustPayrollCommissionCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var payroll = await _db.Payrolls.FirstOrDefaultAsync(x => x.Id == cmd.PayrollId, ct);
        if (payroll is null)
            return Result.Failure(PayrollErrors.NotFound(cmd.PayrollId));

        var detail = await _db.PayrollDetails.FirstOrDefaultAsync(
            x => x.Id == cmd.PayrollDetailId && x.PayrollId == cmd.PayrollId,
            ct
        );

        if (detail is null)
            return Result.Failure(PayrollErrors.DetailNotFound(cmd.PayrollDetailId));

        var before = detail.CommissionAmount;

        detail.CommissionAmount = cmd.CommissionAmount;
        detail.GrossSalary = (detail.GrossSalary - before) + cmd.CommissionAmount;
        detail.NetSalary = detail.GrossSalary - detail.Deductions;
        detail.UpdatedAtUtc = DateTime.UtcNow;

        detail.RaiseCommissionAdjusted();

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Payroll",
            "PayrollDetail",
            detail.Id,
            "PAYROLL_COMMISSION_ADJUSTED",
            $"commission={before}",
            $"commission={detail.CommissionAmount}; payrollId={detail.PayrollId}; employeeId={detail.EmployeeId}",
            ip,
            ua,
            ct
        );

        await _rt.NotifyUserAsync(
            userId,
            "payroll.commission.adjusted",
            new
            {
                payrollId = detail.PayrollId,
                payrollDetailId = detail.Id,
                employeeId = detail.EmployeeId,
                commission = detail.CommissionAmount,
            },
            ct
        );

        return Result.Success();
    }
}
