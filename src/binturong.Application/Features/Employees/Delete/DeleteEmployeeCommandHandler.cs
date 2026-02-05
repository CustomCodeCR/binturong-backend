using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Employees;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Employees.Delete;

internal sealed class DeleteEmployeeCommandHandler : ICommandHandler<DeleteEmployeeCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteEmployeeCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteEmployeeCommand cmd, CancellationToken ct)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(x => x.Id == cmd.EmployeeId, ct);
        if (employee is null)
            return Result.Failure(EmployeeErrors.NotFound(cmd.EmployeeId));

        employee.RaiseDeleted();

        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
