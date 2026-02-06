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

        foreach (var def in ScopeRegistry.All)
        {
            foreach (var roleName in def.DefaultRoles)
            {
                if (!roleByName.TryGetValue(roleName, out var role))
                    continue;

                var scope = scopeByCode[def.Code];

                var exists = await _db.RoleScopes.AnyAsync(
                    x => x.RoleId == role.Id && x.ScopeId == scope.Id,
                    ct
                );

                if (exists)
                    continue;

                var rs = new RoleScope
                {
                    Id = Guid.NewGuid(),
                    RoleId = role.Id,
                    ScopeId = scope.Id,
                };

                rs.RaiseAssigned(scope.Code);
                _db.RoleScopes.Add(rs);
            }
        }

        // SuperAdmin gets everything
        await AssignAllScopesToRoleAsync(ScopeRegistry.Roles.SuperAdmin, ct);

        await _db.SaveChangesAsync(ct);
    }

    private async Task AssignAllScopesToRoleAsync(string roleName, CancellationToken ct)
    {
        var role = await _db.Roles.FirstAsync(x => x.Name == roleName, ct);
        var scopes = await _db.Scopes.AsNoTracking().ToListAsync(ct);

        var existing = await _db
            .RoleScopes.Where(x => x.RoleId == role.Id)
            .Select(x => x.ScopeId)
            .ToListAsync(ct);

        var set = existing.ToHashSet();

        foreach (var s in scopes)
        {
            if (set.Contains(s.Id))
                continue;

            _db.RoleScopes.Add(
                new RoleScope
                {
                    Id = Guid.NewGuid(),
                    RoleId = role.Id,
                    ScopeId = s.Id,
                }
            );
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
