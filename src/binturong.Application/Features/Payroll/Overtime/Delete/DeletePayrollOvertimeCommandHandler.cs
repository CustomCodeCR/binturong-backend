using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Payroll.Overtime.Delete;

internal sealed class DeletePayrollOvertimeCommandHandler
    : ICommandHandler<DeletePayrollOvertimeCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _rt;

    public DeletePayrollOvertimeCommandHandler(
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

    public async Task<Result> Handle(DeletePayrollOvertimeCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var entry = await _db.PayrollOvertimeEntries.FirstOrDefaultAsync(
            x => x.Id == cmd.OvertimeId,
            ct
        );
        if (entry is null)
            return Result.Failure(
                Error.NotFound(
                    "Payroll.Overtime.NotFound",
                    $"Overtime '{cmd.OvertimeId}' not found."
                )
            );

        entry.RaiseDeleted();

        _db.PayrollOvertimeEntries.Remove(entry);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Payroll",
            "PayrollOvertimeEntry",
            entry.Id,
            "PAYROLL_OVERTIME_DELETED",
            $"employeeId={entry.EmployeeId}; workDate={entry.WorkDate}; hours={entry.Hours}",
            string.Empty,
            ip,
            ua,
            ct
        );

        await _rt.NotifyUserAsync(
            userId,
            "payroll.overtime.deleted",
            new { overtimeId = entry.Id, employeeId = entry.EmployeeId },
            ct
        );

        return Result.Success();
    }
}
