using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.Options;
using Domain.PayrollOvertimes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Payroll.Overtime.Register;

internal sealed class RegisterPayrollOvertimeCommandHandler
    : ICommandHandler<RegisterPayrollOvertimeCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly PayrollOptions _opt;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _rt;

    public RegisterPayrollOvertimeCommandHandler(
        IApplicationDbContext db,
        PayrollOptions opt,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser,
        IRealtimeNotifier rt
    )
    {
        _db = db;
        _opt = opt;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
        _rt = rt;
    }

    public async Task<Result<Guid>> Handle(RegisterPayrollOvertimeCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        if (cmd.EmployeeId == Guid.Empty)
            return Result.Failure<Guid>(PayrollErrors.EmployeeNotFound);

        if (cmd.Hours <= 0)
            return Result.Failure<Guid>(PayrollErrors.InvalidOvertimeHours);

        if (cmd.Hours > _opt.MaxOvertimeHoursPerEntry)
            return Result.Failure<Guid>(PayrollErrors.OvertimeTooHigh);

        var employeeExists = await _db.Employees.AnyAsync(x => x.Id == cmd.EmployeeId, ct);
        if (!employeeExists)
            return Result.Failure<Guid>(PayrollErrors.EmployeeNotFound);

        var now = DateTime.UtcNow;

        var entry = new PayrollOvertimeEntry
        {
            Id = Guid.NewGuid(),
            EmployeeId = cmd.EmployeeId,
            WorkDate = cmd.WorkDate,
            Hours = cmd.Hours,
            Notes = cmd.Notes?.Trim() ?? string.Empty,
            CreatedAtUtc = now,
        };

        entry.RaiseRegistered();

        _db.PayrollOvertimeEntries.Add(entry);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Payroll",
            "PayrollOvertimeEntry",
            entry.Id,
            "PAYROLL_OVERTIME_REGISTERED",
            string.Empty,
            $"employeeId={entry.EmployeeId}; workDate={entry.WorkDate}; hours={entry.Hours}",
            ip,
            ua,
            ct
        );

        await _rt.NotifyUserAsync(
            userId,
            "payroll.overtime.registered",
            new
            {
                overtimeId = entry.Id,
                employeeId = entry.EmployeeId,
                hours = entry.Hours,
                workDate = entry.WorkDate,
            },
            ct
        );

        return Result.Success(entry.Id);
    }
}
