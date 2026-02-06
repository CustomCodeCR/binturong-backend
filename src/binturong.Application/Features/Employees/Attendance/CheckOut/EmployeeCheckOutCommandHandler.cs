using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.EmployeeHistory;
using Domain.Employees;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Employees.Attendance.CheckOut;

internal sealed class EmployeeCheckOutCommandHandler : ICommandHandler<EmployeeCheckOutCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public EmployeeCheckOutCommandHandler(
        IApplicationDbContext db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(EmployeeCheckOutCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var employee = await _db
            .Employees.Include(x => x.History)
            .FirstOrDefaultAsync(x => x.Id == cmd.EmployeeId, ct);

        if (employee is null)
        {
            await _bus.AuditAsync(
                userId,
                "Employees",
                "Employee",
                cmd.EmployeeId,
                "EMPLOYEE_CHECK_OUT_FAILED",
                string.Empty,
                $"reason=not_found; employeeId={cmd.EmployeeId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(EmployeeErrors.NotFound(cmd.EmployeeId));
        }

        var lastEvent = employee
            .History.OrderByDescending(h => h.EventDate)
            .ThenByDescending(h => h.Id)
            .FirstOrDefault();

        var hasOpen = lastEvent?.EventType == "CHECK_IN";

        var now = DateTime.UtcNow;
        var result = employee.RegisterCheckOut(hasOpen, now);
        if (result.IsFailure)
        {
            await _bus.AuditAsync(
                userId,
                "Employees",
                "Employee",
                employee.Id,
                "EMPLOYEE_CHECK_OUT_FAILED",
                $"lastEventType={lastEvent?.EventType}; lastEventDate={lastEvent?.EventDate}",
                $"reason=domain_rejected; employeeId={employee.Id}; hasOpen={hasOpen}; error={result.Error}",
                ip,
                ua,
                ct
            );

            return result;
        }

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

        await _bus.AuditAsync(
            userId,
            "Employees",
            "Employee",
            employee.Id,
            "EMPLOYEE_CHECK_OUT",
            $"lastEventType={lastEvent?.EventType}; lastEventDate={lastEvent?.EventDate}",
            $"employeeId={employee.Id}; eventType=CHECK_OUT; at={now:O}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
