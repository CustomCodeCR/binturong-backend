using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Roles;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Roles.Update;

internal sealed class UpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateRoleCommandHandler(
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

    public async Task<Result> Handle(UpdateRoleCommand command, CancellationToken ct)
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
                "ROLE_UPDATE_FAILED",
                string.Empty,
                $"reason=not_found; roleId={command.RoleId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(RoleErrors.NotFound(command.RoleId));
        }

        var before = $"name={role.Name}; description={role.Description}; isActive={role.IsActive}";

        var name = command.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            await _bus.AuditAsync(
                userId,
                "Roles",
                "Role",
                role.Id,
                "ROLE_UPDATE_FAILED",
                before,
                "reason=invalid_name",
                ip,
                ua,
                ct
            );

            return Result.Failure(RoleErrors.InvalidName);
        }

        var exists = await _db.Roles.AnyAsync(
            x => x.Id != command.RoleId && x.Name.ToLower() == name.ToLower(),
            ct
        );
        if (exists)
        {
            await _bus.AuditAsync(
                userId,
                "Roles",
                "Role",
                role.Id,
                "ROLE_UPDATE_FAILED",
                before,
                $"reason=name_not_unique; newName={name}",
                ip,
                ua,
                ct
            );

            return Result.Failure(RoleErrors.NameNotUnique);
        }

        role.Name = name;
        role.Description = command.Description?.Trim();
        role.IsActive = command.IsActive;

        role.RaiseUpdated();

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Roles",
            "Role",
            role.Id,
            "ROLE_UPDATED",
            before,
            $"name={role.Name}; description={role.Description}; isActive={role.IsActive}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
