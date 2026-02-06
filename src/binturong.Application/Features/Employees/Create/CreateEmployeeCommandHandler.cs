using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Employees;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Employees.Create;

internal sealed class CreateEmployeeCommandHandler : ICommandHandler<CreateEmployeeCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public CreateEmployeeCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateEmployeeCommand cmd, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var nationalId = cmd.NationalId.Trim();

        var exists = await _db.Employees.AnyAsync(x => x.NationalId == nationalId, ct);
        if (exists)
        {
            await _bus.AuditAsync(
                userId,
                "Employees",
                "Employee",
                null,
                "EMPLOYEE_CREATE_FAILED",
                string.Empty,
                $"reason=national_id_not_unique; nationalId={nationalId}; fullName={cmd.FullName}; branchId={cmd.BranchId}; userId={cmd.UserId}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(EmployeeErrors.NationalIdNotUnique);
        }

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            UserId = cmd.UserId,
            BranchId = cmd.BranchId,
            FullName = cmd.FullName.Trim(),
            NationalId = nationalId,
            JobTitle = cmd.JobTitle.Trim(),
            BaseSalary = cmd.BaseSalary,
            HireDate = cmd.HireDate,
            TerminationDate = cmd.TerminationDate,
            IsActive = cmd.IsActive,
        };

        employee.RaiseCreated();

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Employees",
            "Employee",
            employee.Id,
            "EMPLOYEE_CREATED",
            string.Empty,
            $"employeeId={employee.Id}; fullName={employee.FullName}; nationalId={employee.NationalId}; jobTitle={employee.JobTitle}; branchId={employee.BranchId}; userId={employee.UserId}; isActive={employee.IsActive}",
            ip,
            ua,
            ct
        );

        return Result.Success(employee.Id);
    }
}
