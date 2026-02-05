using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Employees;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Employees.Update;

internal sealed class UpdateEmployeeCommandHandler : ICommandHandler<UpdateEmployeeCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateEmployeeCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateEmployeeCommand cmd, CancellationToken ct)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(x => x.Id == cmd.EmployeeId, ct);
        if (employee is null)
            return Result.Failure(EmployeeErrors.NotFound(cmd.EmployeeId));

        employee.UserId = cmd.UserId;
        employee.BranchId = cmd.BranchId;
        employee.FullName = cmd.FullName.Trim();
        employee.JobTitle = cmd.JobTitle.Trim();
        employee.BaseSalary = cmd.BaseSalary;
        employee.TerminationDate = cmd.TerminationDate;
        employee.IsActive = cmd.IsActive;

        employee.RaiseUpdated();

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
