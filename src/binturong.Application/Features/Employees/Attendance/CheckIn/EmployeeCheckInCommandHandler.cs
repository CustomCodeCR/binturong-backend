using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.EmployeeHistory;
using Domain.Employees;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Employees.Attendance.CheckIn;

internal sealed class EmployeeCheckInCommandHandler : ICommandHandler<EmployeeCheckInCommand>
{
    private readonly IApplicationDbContext _db;

    public EmployeeCheckInCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(EmployeeCheckInCommand cmd, CancellationToken ct)
    {
        var employee = await _db
            .Employees.Include(x => x.History)
            .FirstOrDefaultAsync(x => x.Id == cmd.EmployeeId, ct);

        if (employee is null)
            return Result.Failure(EmployeeErrors.NotFound(cmd.EmployeeId));

        var hasOpen =
            employee
                .History.OrderByDescending(h => h.EventDate)
                .ThenByDescending(h => h.Id)
                .FirstOrDefault()
                ?.EventType == "CHECK_IN";

        var now = DateTime.UtcNow;
        var result = employee.RegisterCheckIn(hasOpen, now);
        if (result.IsFailure)
            return result;

        _db.EmployeeHistory.Add(
            new EmployeeHistoryEntry
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                EventType = "CHECK_IN",
                Description = "Employee check-in",
                EventDate = DateOnly.FromDateTime(now),
            }
        );

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
