using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using PayrollEntity = Domain.Payrolls.Payroll;

namespace Application.Features.Payroll.Create;

internal sealed class CreatePayrollCommandHandler : ICommandHandler<CreatePayrollCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _rt;

    public CreatePayrollCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreatePayrollCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        if (string.IsNullOrWhiteSpace(cmd.PeriodCode))
            return Result.Failure<Guid>(PayrollErrors.PeriodCodeRequired);

        if (cmd.StartDate > cmd.EndDate)
            return Result.Failure<Guid>(PayrollErrors.InvalidPeriod);

        var periodCode = cmd.PeriodCode.Trim();

        var exists = await _db.Payrolls.AnyAsync(x => x.PeriodCode == periodCode, ct);
        if (exists)
            return Result.Failure<Guid>(
                Error.Conflict(
                    "Payroll.PeriodCodeDuplicate",
                    $"PeriodCode '{periodCode}' already exists."
                )
            );

        var now = DateTime.UtcNow;

        var payroll = new PayrollEntity
        {
            Id = Guid.NewGuid(),
            PeriodCode = periodCode,
            StartDate = cmd.StartDate,
            EndDate = cmd.EndDate,
            PayrollType = cmd.PayrollType.Trim(),
            Status = "Draft",
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        payroll.RaiseCreated();

        _db.Payrolls.Add(payroll);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Payroll",
            "Payroll",
            payroll.Id,
            "PAYROLL_CREATED",
            string.Empty,
            $"payrollId={payroll.Id}; periodCode={payroll.PeriodCode}; start={payroll.StartDate}; end={payroll.EndDate}; type={payroll.PayrollType}; status={payroll.Status}",
            ip,
            ua,
            ct
        );

        await _rt.NotifyUserAsync(
            userId,
            "payroll.created",
            new { payrollId = payroll.Id, periodCode = payroll.PeriodCode },
            ct
        );

        return Result.Success(payroll.Id);
    }
}
