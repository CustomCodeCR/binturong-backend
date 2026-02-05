using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Roles;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Roles.Update;

internal sealed class UpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateRoleCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateRoleCommand command, CancellationToken ct)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(x => x.Id == command.RoleId, ct);
        if (role is null)
            return Result.Failure(RoleErrors.NotFound(command.RoleId));

        var name = command.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(RoleErrors.InvalidName);

        var exists = await _db.Roles.AnyAsync(
            x => x.Id != command.RoleId && x.Name.ToLower() == name.ToLower(),
            ct
        );
        if (exists)
            return Result.Failure(RoleErrors.NameNotUnique);

        role.Name = name;
        role.Description = command.Description?.Trim();
        role.IsActive = command.IsActive;

        role.RaiseUpdated();

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
