using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Domain.Roles;
using Domain.UserRoles;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Users.AssignRole;

internal sealed class AssignRoleToUserCommandHandler : ICommandHandler<AssignRoleToUserCommand>
{
    private const string Module = "Users";
    private const string Entity = "User";

    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public AssignRoleToUserCommandHandler(
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

    public async Task<Result> Handle(AssignRoleToUserCommand command, CancellationToken ct)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(x => x.Id == command.RoleId, ct);
        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound(command.RoleId));
        }

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == command.UserId, ct);
        if (user is null)
        {
            return Result.Failure(
                Error.NotFound("Users.NotFound", $"User '{command.UserId}' not found")
            );
        }

        var currentRoles = await _db
            .UserRoles.Where(x => x.UserId == command.UserId)
            .ToListAsync(ct);

        var selectedAlreadyExists = currentRoles.Any(x => x.RoleId == command.RoleId);

        foreach (var currentRole in currentRoles)
        {
            if (currentRole.RoleId == command.RoleId)
            {
                continue;
            }

            _db.UserRoles.Remove(currentRole);
            user.Raise(new UserRoleRemovedDomainEvent(command.UserId, currentRole.RoleId));
        }

        if (!selectedAlreadyExists)
        {
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                RoleId = command.RoleId,
            };

            _db.UserRoles.Add(userRole);
            user.Raise(new UserRoleAssignedDomainEvent(command.UserId, command.RoleId));
        }

        await _db.SaveChangesAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                command.UserId,
                "USER_ROLE_ASSIGNED",
                string.Empty,
                $"targetUserId={command.UserId}; roleId={command.RoleId}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success();
    }
}
