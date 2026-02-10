using Application.Security;
using Domain.Roles;
using Domain.RoleScopes;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Postgres.Seed;

public sealed class RoleSeeder
{
    private readonly ApplicationDbContext _db;

    public RoleSeeder(ApplicationDbContext db) => _db = db;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await EnsureRoleAsync(ScopeRegistry.Roles.SuperAdmin, "System super administrator", ct);
        await EnsureRoleAsync(ScopeRegistry.Roles.Admin, "System administrator", ct);
        await EnsureRoleAsync(ScopeRegistry.Roles.Manager, "Manager role", ct);
        await EnsureRoleAsync(ScopeRegistry.Roles.Operator, "Operator role", ct);

        await AssignScopesByRegistryAsync(ct);
    }

    private async Task AssignScopesByRegistryAsync(CancellationToken ct)
    {
        var roles = await _db.Roles.AsNoTracking().ToListAsync(ct);
        var scopes = await _db.Scopes.AsNoTracking().ToListAsync(ct);

        var roleByName = roles.ToDictionary(x => x.Name);
        var scopeByCode = scopes.ToDictionary(x => x.Code);

        // Load all existing pairs once (DB)
        var existingPairs = await _db
            .RoleScopes.AsNoTracking()
            .Select(x => new { x.RoleId, x.ScopeId })
            .ToListAsync(ct);

        // Track pairs already in DB + pairs we add in this run
        var assigned = existingPairs.Select(x => (x.RoleId, x.ScopeId)).ToHashSet();

        foreach (var def in ScopeRegistry.All)
        {
            if (!scopeByCode.TryGetValue(def.Code, out var scope))
                continue;

            foreach (var roleName in def.DefaultRoles.Distinct())
            {
                if (!roleByName.TryGetValue(roleName, out var role))
                    continue;

                var key = (role.Id, scope.Id);
                if (assigned.Contains(key))
                    continue;

                var rs = new RoleScope
                {
                    Id = Guid.NewGuid(),
                    RoleId = role.Id,
                    ScopeId = scope.Id,
                };

                rs.RaiseAssigned(scope.Code);

                _db.RoleScopes.Add(rs);
                assigned.Add(key); // critical: prevents duplicates in same run
            }
        }

        // SuperAdmin gets everything (safe with the same HashSet)
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
        if (!roleByName.TryGetValue(roleName, out var role))
        {
            // Fallback: in case roleByName doesn't include it for some reason
            role = await _db.Roles.AsNoTracking().FirstAsync(x => x.Name == roleName, ct);
        }

        foreach (var s in allScopes)
        {
            var key = (role.Id, s.Id);
            if (assigned.Contains(key))
                continue;

            _db.RoleScopes.Add(
                new RoleScope
                {
                    Id = Guid.NewGuid(),
                    RoleId = role.Id,
                    ScopeId = s.Id,
                }
            );

            assigned.Add(key); // critical
        }
    }

    private async Task EnsureRoleAsync(string name, string description, CancellationToken ct)
    {
        if (await _db.Roles.AnyAsync(x => x.Name == name, ct))
            return;

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
}
