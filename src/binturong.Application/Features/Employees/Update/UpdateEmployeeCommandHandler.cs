using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Employees;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Employees.Update;

internal sealed class UpdateEmployeeCommandHandler : ICommandHandler<UpdateEmployeeCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateEmployeeCommandHandler(
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

    public async Task<Result> Handle(UpdateEmployeeCommand cmd, CancellationToken ct)
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
                "EMPLOYEE_UPDATE_FAILED",
                string.Empty,
                $"reason=not_found; employeeId={cmd.EmployeeId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(EmployeeErrors.NotFound(cmd.EmployeeId));
        }

        var before =
            $"userId={employee.UserId}; branchId={employee.BranchId}; fullName={employee.FullName}; jobTitle={employee.JobTitle}; baseSalary={employee.BaseSalary}; terminationDate={employee.TerminationDate}; isActive={employee.IsActive}";

        employee.UserId = cmd.UserId;
        employee.BranchId = cmd.BranchId;
        employee.FullName = cmd.FullName.Trim();
        employee.JobTitle = cmd.JobTitle.Trim();
        employee.BaseSalary = cmd.BaseSalary;
        employee.TerminationDate = cmd.TerminationDate;
        employee.IsActive = cmd.IsActive;

        employee.RaiseUpdated();

        await _db.SaveChangesAsync(ct);

        var after =
            $"userId={employee.UserId}; branchId={employee.BranchId}; fullName={employee.FullName}; jobTitle={employee.JobTitle}; baseSalary={employee.BaseSalary}; terminationDate={employee.TerminationDate}; isActive={employee.IsActive}";

        await _bus.AuditAsync(
            userId,
            "Employees",
            "Employee",
            employee.Id,
            "EMPLOYEE_UPDATED",
            before,
            after,
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
