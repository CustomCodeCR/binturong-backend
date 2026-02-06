using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Employees;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Employees.Delete;

internal sealed class DeleteEmployeeCommandHandler : ICommandHandler<DeleteEmployeeCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeleteEmployeeCommandHandler(
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

    public async Task<Result> Handle(DeleteEmployeeCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var employee = await _db.Employees.FirstOrDefaultAsync(x => x.Id == cmd.EmployeeId, ct);
        if (employee is null)
        {
            await _bus.AuditAsync(
                userId,
                "Employees",
                "Employee",
                cmd.EmployeeId,
                "EMPLOYEE_DELETE_FAILED",
                string.Empty,
                $"reason=not_found; employeeId={cmd.EmployeeId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(EmployeeErrors.NotFound(cmd.EmployeeId));
        }

        var before =
            $"fullName={employee.FullName}; nationalId={employee.NationalId}; jobTitle={employee.JobTitle}; branchId={employee.BranchId}; userId={employee.UserId}; isActive={employee.IsActive}";

        employee.RaiseDeleted();

        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Employees",
            "Employee",
            employee.Id,
            "EMPLOYEE_DELETED",
            before,
            $"employeeId={employee.Id}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
