using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.Options;
using Domain.PayrollDetails;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using PayrollEntity = Domain.Payrolls.Payroll;

namespace Application.Features.Payroll.Calculate;

internal sealed class CalculatePayrollCommandHandler
    : ICommandHandler<CalculatePayrollCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly PayrollOptions _opt;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _rt;

    public CalculatePayrollCommandHandler(
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

    public async Task<Result<Guid>> Handle(CalculatePayrollCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        if (string.IsNullOrWhiteSpace(cmd.PeriodCode))
            return Result.Failure<Guid>(PayrollErrors.PeriodCodeRequired);

        if (cmd.StartDate > cmd.EndDate)
            return Result.Failure<Guid>(PayrollErrors.InvalidPeriod);

        if (!cmd.AttendanceConfirmed)
            return Result.Failure<Guid>(PayrollErrors.AttendanceNotConfirmed);

        var now = DateTime.UtcNow;

        var payroll = new PayrollEntity
        {
            Id = Guid.NewGuid(),
            PeriodCode = cmd.PeriodCode.Trim(),
            StartDate = cmd.StartDate,
            EndDate = cmd.EndDate,
            PayrollType = cmd.PayrollType.Trim(),
            Status = "Calculated",
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        var employees = await _db.Employees.Where(x => x.IsActive).ToListAsync(ct);

        var startUtc = new DateTime(
            cmd.StartDate.Year,
            cmd.StartDate.Month,
            cmd.StartDate.Day,
            0,
            0,
            0,
            DateTimeKind.Utc
        );
        var endUtc = new DateTime(
            cmd.EndDate.Year,
            cmd.EndDate.Month,
            cmd.EndDate.Day,
            23,
            59,
            59,
            DateTimeKind.Utc
        );

        var overtimeByEmployee = await _db
            .PayrollOvertimeEntries.Where(x =>
                x.WorkDate >= cmd.StartDate && x.WorkDate <= cmd.EndDate
            )
            .GroupBy(x => x.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, Hours = g.Sum(x => x.Hours) })
            .ToListAsync(ct);

        var overtimeMap = overtimeByEmployee.ToDictionary(x => x.EmployeeId, x => x.Hours);

        var salesBySeller = await _db
            .SalesOrders.Where(x =>
                x.Status == "Confirmed" && x.OrderDate >= startUtc && x.OrderDate <= endUtc
            )
            .GroupBy(x => x.SellerUserId)
            .Select(g => new { SellerUserId = g.Key, Total = g.Sum(x => x.Total) })
            .ToListAsync(ct);

        var salesMap = salesBySeller
            .Where(x => x.SellerUserId.HasValue)
            .ToDictionary(x => x.SellerUserId!.Value, x => x.Total);

        foreach (var emp in employees)
        {
            var overtimeHours = overtimeMap.TryGetValue(emp.Id, out var h) ? h : 0m;

            decimal commissions = 0m;
            if (
                emp.UserId is not null
                && salesMap.TryGetValue(emp.UserId.Value, out var salesTotal)
            )
                commissions = salesTotal * (_opt.CommissionPercent / 100m);

            var overtimePay = overtimeHours * _opt.OvertimeHourlyRate;

            var gross = emp.BaseSalary + overtimePay + commissions;
            var deductions = gross * (_opt.DeductionsPercent / 100m);
            var employerContrib = gross * (_opt.EmployerContribPercent / 100m);
            var net = gross - deductions;

            var detail = new PayrollDetail
            {
                Id = Guid.NewGuid(),
                PayrollId = payroll.Id,
                EmployeeId = emp.Id,
                GrossSalary = gross,
                OvertimeHours = overtimeHours,
                CommissionAmount = commissions,
                Deductions = deductions,
                EmployerContrib = employerContrib,
                NetSalary = net,
                UpdatedAtUtc = now,
            };

            detail.RaiseCalculated();
            payroll.Details.Add(detail);
        }

        payroll.RaiseCreated();
        payroll.RaiseCalculated();

        _db.Payrolls.Add(payroll);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Payroll",
            "Payroll",
            payroll.Id,
            "PAYROLL_CALCULATED",
            string.Empty,
            $"periodCode={payroll.PeriodCode}; start={payroll.StartDate}; end={payroll.EndDate}; type={payroll.PayrollType}; details={payroll.Details.Count}",
            ip,
            ua,
            ct
        );

        await _rt.NotifyUserAsync(
            userId,
            "payroll.calculated",
            new
            {
                payrollId = payroll.Id,
                periodCode = payroll.PeriodCode,
                count = payroll.Details.Count,
            },
            ct
        );

        return Result.Success(payroll.Id);
    }
}
