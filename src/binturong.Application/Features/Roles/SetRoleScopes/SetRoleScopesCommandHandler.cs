using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.Roles;
using Domain.RoleScopes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Roles.SetRoleScopes;

internal sealed class SetRoleScopesCommandHandler : ICommandHandler<SetRoleScopesCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public SetRoleScopesCommandHandler(
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

    public async Task<Result> Handle(SetRoleScopesCommand command, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;

        var role = await _db
            .Roles.Include(x => x.RoleScopes)
            .FirstOrDefaultAsync(x => x.Id == command.RoleId, ct);

        if (role is null)
        {
            await _bus.AuditAsync(
                userId,
                "Roles",
                "Role",
                command.RoleId,
                "ROLE_SCOPES_SET_FAILED",
                string.Empty,
                $"reason=not_found; roleId={command.RoleId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(RoleErrors.NotFound(command.RoleId));
        }

        var scopes = await _db.Scopes.Where(s => command.ScopeIds.Contains(s.Id)).ToListAsync(ct);
        if (scopes.Count != command.ScopeIds.Count)
        {
            await _bus.AuditAsync(
                userId,
                "Roles",
                "Role",
                role.Id,
                "ROLE_SCOPES_SET_FAILED",
                string.Empty,
                "reason=scopes_invalid_one_or_more_missing",
                ip,
                ua,
                ct
            );

            return Result.Failure(
                Error.Failure("Roles.Scopes.Invalid", "One or more scopes do not exist")
            );
        }

        var before =
            $"roleId={role.Id}; scopeIds=[{string.Join(",", role.RoleScopes.Select(x => x.ScopeId))}]";
        var desired = command.ScopeIds.ToHashSet();
        var existing = role.RoleScopes.Select(rs => rs.ScopeId).ToHashSet();

        var toRemove = role.RoleScopes.Where(rs => !desired.Contains(rs.ScopeId)).ToList();
        foreach (var rs in toRemove)
        {
            var sc =
                scopes.FirstOrDefault(x => x.Id == rs.ScopeId)
                ?? await _db.Scopes.FirstAsync(x => x.Id == rs.ScopeId, ct);

            role.Raise(new RoleScopeRemovedDomainEvent(role.Id, rs.ScopeId, sc.Code));
            _db.RoleScopes.Remove(rs);
        }

        var toAdd = scopes.Where(s => !existing.Contains(s.Id)).ToList();
        foreach (var sc in toAdd)
        {
            var rs = new RoleScope
            {
                Id = Guid.NewGuid(),
                RoleId = role.Id,
                ScopeId = sc.Id,
            };

            role.Raise(new RoleScopeAssignedDomainEvent(role.Id, sc.Id, sc.Code));
            _db.RoleScopes.Add(rs);
        }

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Roles",
            "Role",
            role.Id,
            "ROLE_SCOPES_SET",
            before,
            $"roleId={role.Id}; scopeIds=[{string.Join(",", command.ScopeIds)}]",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
