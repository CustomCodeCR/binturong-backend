using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.Options;
using Domain.PayrollDetails;
using Domain.PayrollOvertimes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Payroll.Overtime.Register;

internal sealed class RegisterPayrollOvertimeCommandHandler
    : ICommandHandler<RegisterPayrollOvertimeCommand, Guid>
{
    private const decimal EmployeeCcssPercent = 10.83m;
    private const decimal StandardDailyHours = 8m;
    private const decimal StandardMonthlyDays = 30m;
    private const decimal OvertimeMultiplier = 1.5m;

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

        var employee = await _db.Employees.FirstOrDefaultAsync(x => x.Id == cmd.EmployeeId, ct);

        if (employee is null)
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

        var affectedPayrolls = await _db
            .Payrolls.Include(x => x.Details)
            .Where(x =>
                x.StartDate <= cmd.WorkDate && x.EndDate >= cmd.WorkDate && x.Status != "Closed"
            )
            .ToListAsync(ct);

        foreach (var payroll in affectedPayrolls)
        {
            await RecalculateEmployeePayrollDetailAsync(payroll, employee, now, ct);
        }

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Payroll",
            "PayrollOvertimeEntry",
            entry.Id,
            "PAYROLL_OVERTIME_REGISTERED",
            string.Empty,
            $"employeeId={entry.EmployeeId}; workDate={entry.WorkDate}; hours={entry.Hours}; affectedPayrolls={affectedPayrolls.Count}",
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
                affectedPayrolls = affectedPayrolls.Select(x => x.Id).ToArray(),
            },
            ct
        );

        return Result.Success(entry.Id);
    }

    private async Task RecalculateEmployeePayrollDetailAsync(
        Domain.Payrolls.Payroll payroll,
        Domain.Employees.Employee employee,
        DateTime now,
        CancellationToken ct
    )
    {
        var startUtc = new DateTime(
            payroll.StartDate.Year,
            payroll.StartDate.Month,
            payroll.StartDate.Day,
            0,
            0,
            0,
            DateTimeKind.Utc
        );

        var endUtc = new DateTime(
            payroll.EndDate.Year,
            payroll.EndDate.Month,
            payroll.EndDate.Day,
            23,
            59,
            59,
            DateTimeKind.Utc
        );

        var overtimeHours =
            await _db
                .PayrollOvertimeEntries.Where(x =>
                    x.EmployeeId == employee.Id
                    && x.WorkDate >= payroll.StartDate
                    && x.WorkDate <= payroll.EndDate
                )
                .SumAsync(x => (decimal?)x.Hours, ct) ?? 0m;

        decimal commissions = 0m;

        if (employee.UserId is not null)
        {
            var salesTotal =
                await _db
                    .SalesOrders.Where(x =>
                        x.SellerUserId == employee.UserId
                        && x.Status == "Confirmed"
                        && x.OrderDate >= startUtc
                        && x.OrderDate <= endUtc
                    )
                    .SumAsync(x => (decimal?)x.Total, ct) ?? 0m;

            commissions = salesTotal * (_opt.CommissionPercent / 100m);
        }

        var normalHourlyRate = CalculateNormalHourlyRate(employee.BaseSalary);
        var overtimeHourlyRate = normalHourlyRate * OvertimeMultiplier;
        var overtimePay = RoundCurrency(overtimeHours * overtimeHourlyRate);

        var gross = employee.BaseSalary + overtimePay + commissions;

        var employeeCcss = CalculateEmployeeCcss(gross);
        var incomeTax = CalculateCostaRicaSalaryTax(gross);
        var deductions = employeeCcss + incomeTax;

        var employerContrib = RoundCurrency(gross * (_opt.EmployerContribPercent / 100m));
        var net = gross - deductions;

        var detail = payroll.Details.FirstOrDefault(x => x.EmployeeId == employee.Id);

        if (detail is null)
        {
            detail = new PayrollDetail
            {
                Id = Guid.NewGuid(),
                PayrollId = payroll.Id,
                EmployeeId = employee.Id,
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
            payroll.UpdatedAtUtc = now;
            return;
        }

        detail.GrossSalary = gross;
        detail.OvertimeHours = overtimeHours;
        detail.CommissionAmount = commissions;
        detail.Deductions = deductions;
        detail.EmployerContrib = employerContrib;
        detail.NetSalary = net;
        detail.UpdatedAtUtc = now;

        detail.RaiseCalculated();
        payroll.UpdatedAtUtc = now;
    }

    private static decimal CalculateNormalHourlyRate(decimal baseSalary)
    {
        if (baseSalary <= 0m)
            return 0m;

        return RoundCurrency(baseSalary / StandardMonthlyDays / StandardDailyHours);
    }

    private static decimal CalculateEmployeeCcss(decimal grossSalary)
    {
        if (grossSalary <= 0m)
            return 0m;

        return RoundCurrency(grossSalary * (EmployeeCcssPercent / 100m));
    }

    private static decimal CalculateCostaRicaSalaryTax(decimal grossSalary)
    {
        if (grossSalary <= 918000m)
            return 0m;

        decimal tax = 0m;

        tax += CalculateBracketTax(grossSalary, 918000m, 1347000m, 0.10m);
        tax += CalculateBracketTax(grossSalary, 1347000m, 2364000m, 0.15m);
        tax += CalculateBracketTax(grossSalary, 2364000m, 4727000m, 0.20m);
        tax += CalculateBracketTax(grossSalary, 4727000m, null, 0.25m);

        return RoundCurrency(tax);
    }

    private static decimal CalculateBracketTax(
        decimal grossSalary,
        decimal lowerLimitExclusive,
        decimal? upperLimitInclusive,
        decimal rate
    )
    {
        if (grossSalary <= lowerLimitExclusive)
            return 0m;

        var taxableAmount = upperLimitInclusive.HasValue
            ? Math.Min(grossSalary, upperLimitInclusive.Value) - lowerLimitExclusive
            : grossSalary - lowerLimitExclusive;

        if (taxableAmount <= 0m)
            return 0m;

        return taxableAmount * rate;
    }

    private static decimal RoundCurrency(decimal amount)
    {
        return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
}
