using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Payroll.Update;

internal sealed class UpdatePayrollCommandHandler : ICommandHandler<UpdatePayrollCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _rt;

    public UpdatePayrollCommandHandler(
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

    public async Task<Result> Handle(UpdatePayrollCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        if (string.IsNullOrWhiteSpace(cmd.PeriodCode))
            return Result.Failure(PayrollErrors.PeriodCodeRequired);

        if (cmd.StartDate > cmd.EndDate)
            return Result.Failure(PayrollErrors.InvalidPeriod);

        var payroll = await _db
            .Payrolls.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == cmd.PayrollId, ct);

        if (payroll is null)
            return Result.Failure(PayrollErrors.NotFound(cmd.PayrollId));

        var duplicate = await _db.Payrolls.AnyAsync(
            x => x.Id != cmd.PayrollId && x.PeriodCode == cmd.PeriodCode.Trim(),
            ct
        );

        if (duplicate)
            return Result.Failure(
                Error.Conflict(
                    "Payroll.PeriodCodeDuplicate",
                    $"PeriodCode '{cmd.PeriodCode}' already exists."
                )
            );

        var before =
            $"periodCode={payroll.PeriodCode}; start={payroll.StartDate}; end={payroll.EndDate}; type={payroll.PayrollType}; status={payroll.Status}";

        payroll.PeriodCode = cmd.PeriodCode.Trim();
        payroll.StartDate = cmd.StartDate;
        payroll.EndDate = cmd.EndDate;
        payroll.PayrollType = cmd.PayrollType.Trim();
        payroll.Status = cmd.Status.Trim();
        payroll.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Payroll",
            "Payroll",
            payroll.Id,
            "PAYROLL_UPDATED",
            before,
            $"periodCode={payroll.PeriodCode}; start={payroll.StartDate}; end={payroll.EndDate}; type={payroll.PayrollType}; status={payroll.Status}",
            ip,
            ua,
            ct
        );

        await _rt.NotifyUserAsync(
            userId,
            "payroll.updated",
            new
            {
                payrollId = payroll.Id,
                periodCode = payroll.PeriodCode,
                status = payroll.Status,
            },
            ct
        );

        return Result.Success();
    }
}
