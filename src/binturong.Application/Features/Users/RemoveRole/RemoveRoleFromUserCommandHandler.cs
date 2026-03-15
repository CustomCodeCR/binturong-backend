using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Users.RemoveRole;

internal sealed class RemoveRoleFromUserCommandHandler : ICommandHandler<RemoveRoleFromUserCommand>
{
    private const string Module = "Users";
    private const string Entity = "User";

    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RemoveRoleFromUserCommandHandler(
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

    public async Task<Result> Handle(RemoveRoleFromUserCommand command, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == command.UserId, ct);
        if (user is null)
        {
            return Result.Failure(
                Error.NotFound("Users.NotFound", $"User '{command.UserId}' not found")
            );
        }

        var userRole = await _db.UserRoles.FirstOrDefaultAsync(
            x => x.UserId == command.UserId && x.RoleId == command.RoleId,
            ct
        );

        if (userRole is null)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    _currentUser.UserId,
                    Module,
                    Entity,
                    command.UserId,
                    "USER_ROLE_REMOVE_NOOP",
                    string.Empty,
                    $"targetUserId={command.UserId}; roleId={command.RoleId}",
                    _request.IpAddress,
                    _request.UserAgent
                ),
                ct
            );

            return Result.Success();
        }

        _db.UserRoles.Remove(userRole);
        user.Raise(new Domain.UserRoles.UserRoleRemovedDomainEvent(command.UserId, command.RoleId));

        await _db.SaveChangesAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                command.UserId,
                "USER_ROLE_REMOVED",
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
