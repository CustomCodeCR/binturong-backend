using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Payroll.History;

internal sealed class GetEmployeePaymentHistoryQueryHandler
    : IQueryHandler<GetEmployeePaymentHistoryQuery, IReadOnlyList<EmployeePaymentHistoryRow>>
{
    private readonly IApplicationDbContext _db;

    public GetEmployeePaymentHistoryQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<EmployeePaymentHistoryRow>>> Handle(
        GetEmployeePaymentHistoryQuery q,
        CancellationToken ct
    )
    {
        if (q.EmployeeId == Guid.Empty)
            return Result.Failure<IReadOnlyList<EmployeePaymentHistoryRow>>(
                Error.Validation("Payroll.EmployeeIdRequired", "EmployeeId is required.")
            );

        var query =
            from d in _db.PayrollDetails
            join p in _db.Payrolls on d.PayrollId equals p.Id
            where d.EmployeeId == q.EmployeeId
            select new { p, d };

        if (q.FromUtc is not null)
            query = query.Where(x =>
                new DateTime(
                    x.p.StartDate.Year,
                    x.p.StartDate.Month,
                    x.p.StartDate.Day,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc
                ) >= q.FromUtc.Value
            );

        if (q.ToUtc is not null)
            query = query.Where(x =>
                new DateTime(
                    x.p.EndDate.Year,
                    x.p.EndDate.Month,
                    x.p.EndDate.Day,
                    23,
                    59,
                    59,
                    DateTimeKind.Utc
                ) <= q.ToUtc.Value
            );

        var list = await query
            .OrderByDescending(x => x.p.StartDate)
            .Select(x => new EmployeePaymentHistoryRow
            {
                PayrollId = x.p.Id,
                PeriodCode = x.p.PeriodCode,
                StartDateUtc = new DateTime(
                    x.p.StartDate.Year,
                    x.p.StartDate.Month,
                    x.p.StartDate.Day,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc
                ),
                EndDateUtc = new DateTime(
                    x.p.EndDate.Year,
                    x.p.EndDate.Month,
                    x.p.EndDate.Day,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc
                ),
                Status = x.p.Status,
                GrossSalary = x.d.GrossSalary,
                OvertimeHours = x.d.OvertimeHours,
                CommissionAmount = x.d.CommissionAmount,
                Deductions = x.d.Deductions,
                EmployerContrib = x.d.EmployerContrib,
                NetSalary = x.d.NetSalary,
            })
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<EmployeePaymentHistoryRow>>(list);
    }
}
