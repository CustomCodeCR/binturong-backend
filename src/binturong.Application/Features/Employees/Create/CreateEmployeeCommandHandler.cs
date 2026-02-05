using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Employees;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Employees.Create;

internal sealed class CreateEmployeeCommandHandler : ICommandHandler<CreateEmployeeCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateEmployeeCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateEmployeeCommand cmd, CancellationToken ct)
    {
        var nationalId = cmd.NationalId.Trim();

        var exists = await _db.Employees.AnyAsync(x => x.NationalId == nationalId, ct);
        if (exists)
            return Result.Failure<Guid>(EmployeeErrors.NationalIdNotUnique);

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

        return Result.Success(employee.Id);
    }
}
