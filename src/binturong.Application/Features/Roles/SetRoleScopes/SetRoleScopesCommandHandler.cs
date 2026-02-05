using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Roles;
using Domain.RoleScopes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Roles.SetRoleScopes;

internal sealed class SetRoleScopesCommandHandler : ICommandHandler<SetRoleScopesCommand>
{
    private readonly IApplicationDbContext _db;

    public SetRoleScopesCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(SetRoleScopesCommand command, CancellationToken ct)
    {
        var role = await _db
            .Roles.Include(x => x.RoleScopes)
            .FirstOrDefaultAsync(x => x.Id == command.RoleId, ct);

        if (role is null)
            return Result.Failure(RoleErrors.NotFound(command.RoleId));

        var scopes = await _db.Scopes.Where(s => command.ScopeIds.Contains(s.Id)).ToListAsync(ct);

        if (scopes.Count != command.ScopeIds.Count)
            return Result.Failure(
                Error.Failure("Roles.Scopes.Invalid", "One or more scopes do not exist")
            );

        var existing = role.RoleScopes.Select(rs => rs.ScopeId).ToHashSet();
        var desired = command.ScopeIds.ToHashSet();

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
        return Result.Success();
    }
}
