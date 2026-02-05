using Domain.Roles;
using Domain.RoleScopes;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Postgres.Seed;

public sealed class RoleSeeder
{
    private readonly Database.Postgres.ApplicationDbContext _db;

    public RoleSeeder(Database.Postgres.ApplicationDbContext db) => _db = db;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await EnsureRoleAsync("SuperAdmin", "System super administrator", ct);
        await EnsureRoleAsync("Admin", "System administrator", ct);
        await EnsureRoleAsync("Manager", "Manager role", ct);
        await EnsureRoleAsync("Operator", "Operator role", ct);

        await AssignAllScopesToRoleAsync("SuperAdmin", ct);

        await AssignScopesToRoleAsync(
            "Admin",
            new[]
            {
                "security.scopes.read",
                "security.admin.reset_password",
                "users.read",
                "users.create",
                "users.update",
                "users.delete",
                "roles.read",
                "roles.create",
                "roles.update",
                "roles.delete",
                "roles.scopes.assign",
                "users.roles.assign",
                "branches.read",
                "branches.create",
                "branches.update",
                "branches.delete",
                "warehouses.read",
                "warehouses.create",
                "warehouses.update",
                "warehouses.delete",
                "products.read",
                "products.create",
                "products.update",
                "products.delete",
                "categories.read",
                "categories.create",
                "categories.update",
                "categories.delete",
                "taxes.read",
                "taxes.create",
                "taxes.update",
                "taxes.delete",
                "uoms.read",
                "uoms.create",
                "uoms.update",
                "uoms.delete",
                "inventory.movements.create",
                "inventory.transfers.read",
                "inventory.transfers.create",
                "inventory.transfers.update",
                "inventory.transfers.delete",
                "inventory.transfers.request_review",
                "inventory.transfers.approve",
                "inventory.transfers.reject",
                "inventory.transfers.confirm",
                "inventory.transfers.cancel",
                "inventory.by_branch.read",
                "inventory.stock.read",
                "suppliers.read",
                "suppliers.create",
                "suppliers.update",
                "suppliers.delete",
                "suppliers.credit.assign",
                "clients.read",
                "clients.create",
                "clients.update",
                "clients.delete",
                "employees.read",
                "employees.create",
                "employees.update",
                "employees.delete",
                "employees.attendance.checkin",
                "employees.attendance.checkout",
            },
            ct
        );

        await AssignScopesToRoleAsync(
            "Manager",
            new[]
            {
                "branches.read",
                "warehouses.read",
                "products.read",
                "categories.read",
                "taxes.read",
                "uoms.read",
                "inventory.movements.create",
                "inventory.transfers.read",
                "inventory.transfers.create",
                "inventory.transfers.request_review",
                "inventory.by_branch.read",
                "inventory.stock.read",
                "suppliers.read",
                "clients.read",
                "employees.read",
            },
            ct
        );

        await AssignScopesToRoleAsync(
            "Operator",
            new[]
            {
                "products.read",
                "inventory.movements.create",
                "inventory.transfers.read",
                "inventory.transfers.create",
                "inventory.by_branch.read",
                "employees.attendance.checkin",
                "employees.attendance.checkout",
            },
            ct
        );
    }

    private async Task EnsureRoleAsync(string name, string description, CancellationToken ct)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(x => x.Name == name, ct);
        if (role is not null)
            return;

        var roles = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsActive = true,
        };

        roles.RaiseCreated();
        _db.Roles.Add(roles);

        await _db.SaveChangesAsync(ct);
    }

    private async Task AssignAllScopesToRoleAsync(string roleName, CancellationToken ct)
    {
        var role = await _db.Roles.FirstAsync(x => x.Name == roleName, ct);
        var allScopes = await _db.Scopes.AsNoTracking().ToListAsync(ct);

        var existing = await _db
            .RoleScopes.Where(x => x.RoleId == role.Id)
            .Select(x => x.ScopeId)
            .ToListAsync(ct);

        var set = existing.ToHashSet();

        foreach (var s in allScopes)
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

        await _db.SaveChangesAsync(ct);
    }

    private async Task AssignScopesToRoleAsync(
        string roleName,
        IReadOnlyList<string> scopeCodes,
        CancellationToken ct
    )
    {
        var role = await _db.Roles.FirstAsync(x => x.Name == roleName, ct);

        var scopes = await _db.Scopes.Where(x => scopeCodes.Contains(x.Code)).ToListAsync(ct);

        var existing = await _db
            .RoleScopes.Where(x => x.RoleId == role.Id)
            .Select(x => x.ScopeId)
            .ToListAsync(ct);

        var set = existing.ToHashSet();

        foreach (var s in scopes)
        {
            if (set.Contains(s.Id))
                continue;

            var rs = new RoleScope
            {
                Id = Guid.NewGuid(),
                RoleId = role.Id,
                ScopeId = s.Id,
            };

            rs.RaiseAssigned(s.Code);
            _db.RoleScopes.Add(rs);
        }

        await _db.SaveChangesAsync(ct);
    }
}
