using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Roles;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Roles.Delete;

internal sealed class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeleteRoleCommandHandler(
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

    public async Task<Result> Handle(DeleteRoleCommand command, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;

        var role = await _db.Roles.FirstOrDefaultAsync(x => x.Id == command.RoleId, ct);
        if (role is null)
        {
            await _bus.AuditAsync(
                userId,
                "Roles",
                "Role",
                command.RoleId,
                "ROLE_DELETE_FAILED",
                string.Empty,
                $"reason=not_found; roleId={command.RoleId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(RoleErrors.NotFound(command.RoleId));
        }

        if (role.Name == "SuperAdmin")
        {
            await _bus.AuditAsync(
                userId,
                "Roles",
                "Role",
                role.Id,
                "ROLE_DELETE_FAILED",
                $"name={role.Name}",
                "reason=cannot_delete_system_role",
                ip,
                ua,
                ct
            );

            return Result.Failure(RoleErrors.CannotDeleteSystemRole);
        }

        var before = $"name={role.Name}; description={role.Description}; isActive={role.IsActive}";

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Roles",
            "Role",
            role.Id,
            "ROLE_DELETED",
            before,
            $"roleId={role.Id}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
