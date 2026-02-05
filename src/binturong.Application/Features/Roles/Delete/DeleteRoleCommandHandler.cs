using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Roles;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Roles.Delete;

internal sealed class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteRoleCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteRoleCommand command, CancellationToken ct)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(x => x.Id == command.RoleId, ct);
        if (role is null)
            return Result.Failure(RoleErrors.NotFound(command.RoleId));

        if (role.Name == "SuperAdmin")
            return Result.Failure(RoleErrors.CannotDeleteSystemRole);

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
