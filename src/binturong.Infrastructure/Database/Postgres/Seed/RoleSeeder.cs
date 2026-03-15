using Application.Security;
using Domain.Roles;
using Domain.RoleScopes;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Postgres.Seed;

public sealed class RoleSeeder
{
    private readonly ApplicationDbContext _db;

    public RoleSeeder(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await EnsureRoleAsync(ScopeRegistry.Roles.SuperAdmin, "System super administrator", ct);
        await EnsureRoleAsync(ScopeRegistry.Roles.Admin, "System administrator", ct);
        await EnsureRoleAsync(ScopeRegistry.Roles.Manager, "Manager role", ct);
        await EnsureRoleAsync(ScopeRegistry.Roles.Operator, "Operator role", ct);

        await AssignScopesByRegistryAsync(ct);
    }

    private async Task EnsureRoleAsync(string name, string description, CancellationToken ct)
    {
        var existingRole = await _db.Roles.FirstOrDefaultAsync(x => x.Name == name, ct);
        if (existingRole is not null)
        {
            return;
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsActive = true,
        };

        role.RaiseCreated();

        _db.Roles.Add(role);
        await _db.SaveChangesAsync(ct);
    }

    private async Task AssignScopesByRegistryAsync(CancellationToken ct)
    {
        var roles = await _db.Roles.ToListAsync(ct);
        var scopes = await _db.Scopes.ToListAsync(ct);

        var roleByName = roles.GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.First());

        var scopeByCode = scopes.GroupBy(x => x.Code).ToDictionary(x => x.Key, x => x.First());

        var existingPairs = await _db
            .RoleScopes.Select(x => new { x.RoleId, x.ScopeId })
            .ToListAsync(ct);

        var assigned = existingPairs.Select(x => (x.RoleId, x.ScopeId)).ToHashSet();

        foreach (var def in ScopeRegistry.All)
        {
            if (!scopeByCode.TryGetValue(def.Code, out var scope))
            {
                continue;
            }

            foreach (var roleName in def.DefaultRoles.Distinct())
            {
                if (!roleByName.TryGetValue(roleName, out var role))
                {
                    continue;
                }

                var key = (role.Id, scope.Id);
                if (assigned.Contains(key))
                {
                    continue;
                }

                var roleScope = new RoleScope
                {
                    Id = Guid.NewGuid(),
                    RoleId = role.Id,
                    ScopeId = scope.Id,
                };

                roleScope.RaiseAssigned(scope.Code);

                _db.RoleScopes.Add(roleScope);
                assigned.Add(key);
            }
        }

        await AssignAllScopesToRoleAsync(
            ScopeRegistry.Roles.SuperAdmin,
            scopes,
            roleByName,
            assigned,
            ct
        );

        await _db.SaveChangesAsync(ct);
    }

    private async Task AssignAllScopesToRoleAsync(
        string roleName,
        List<Domain.Scopes.Scope> allScopes,
        Dictionary<string, Role> roleByName,
        HashSet<(Guid RoleId, Guid ScopeId)> assigned,
        CancellationToken ct
    )
    {
        Role role;

        if (!roleByName.TryGetValue(roleName, out var foundRole))
        {
            role = await _db.Roles.FirstAsync(x => x.Name == roleName, ct);
        }
        else
        {
            role = foundRole;
        }

        foreach (var scope in allScopes)
        {
            var key = (role.Id, scope.Id);
            if (assigned.Contains(key))
            {
                continue;
            }

            var roleScope = new RoleScope
            {
                Id = Guid.NewGuid(),
                RoleId = role.Id,
                ScopeId = scope.Id,
            };

            roleScope.RaiseAssigned(scope.Code);

            _db.RoleScopes.Add(roleScope);
            assigned.Add(key);
        }
    }
}
