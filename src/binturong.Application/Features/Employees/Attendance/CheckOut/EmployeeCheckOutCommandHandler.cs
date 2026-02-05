using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.EmployeeHistory;
using Domain.Employees;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Employees.Attendance.CheckOut;

internal sealed class EmployeeCheckOutCommandHandler : ICommandHandler<EmployeeCheckOutCommand>
{
    private readonly IApplicationDbContext _db;

    public EmployeeCheckOutCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(EmployeeCheckOutCommand cmd, CancellationToken ct)
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
        var result = employee.RegisterCheckOut(hasOpen, now);
        if (result.IsFailure)
            return result;

        _db.EmployeeHistory.Add(
            new EmployeeHistoryEntry
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                EventType = "CHECK_OUT",
                Description = "Employee check-out",
                EventDate = DateOnly.FromDateTime(now),
            }
        );

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
