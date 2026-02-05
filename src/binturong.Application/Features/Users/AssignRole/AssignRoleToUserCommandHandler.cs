using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Roles;
using Domain.UserRoles;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Users.AssignRole;

internal sealed class AssignRoleToUserCommandHandler : ICommandHandler<AssignRoleToUserCommand>
{
    private readonly IApplicationDbContext _db;

    public AssignRoleToUserCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(AssignRoleToUserCommand command, CancellationToken ct)
    {
        var roleExists = await _db.Roles.AnyAsync(x => x.Id == command.RoleId, ct);
        if (!roleExists)
            return Result.Failure(RoleErrors.NotFound(command.RoleId));

        var userExists = await _db.Users.AnyAsync(x => x.Id == command.UserId, ct);
        if (!userExists)
            return Result.Failure(
                Error.NotFound("Users.NotFound", $"User '{command.UserId}' not found")
            );

        var current = await _db.UserRoles.Where(x => x.UserId == command.UserId).ToListAsync(ct);

        if (command.ReplaceExisting)
        {
            foreach (var ur in current)
            {
                _db.UserRoles.Remove(ur);
                var tmp = new Domain.Users.User { Id = command.UserId };
                tmp.Raise(new UserRoleRemovedDomainEvent(command.UserId, ur.RoleId));
            }
        }
        else
        {
            if (current.Any(x => x.RoleId == command.RoleId))
                return Result.Failure(UserRoleErrors.Duplicate);
        }

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            RoleId = command.RoleId,
        };

        var u = new Domain.Users.User { Id = command.UserId };
        u.Raise(new UserRoleAssignedDomainEvent(command.UserId, command.RoleId));

        _db.UserRoles.Add(userRole);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
