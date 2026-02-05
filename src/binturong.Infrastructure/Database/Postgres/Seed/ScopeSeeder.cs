using Domain.Scopes;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Postgres.Seed;

public sealed class ScopeSeeder
{
    private readonly Database.Postgres.ApplicationDbContext _db;

    public ScopeSeeder(Database.Postgres.ApplicationDbContext db) => _db = db;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var desired = GetDesiredScopes();

        var existingCodes = await _db.Scopes.AsNoTracking().Select(x => x.Code).ToListAsync(ct);

        var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        foreach (var s in desired)
        {
            if (existing.Contains(s.Code))
                continue;

            var scope = new Scope
            {
                Id = Guid.NewGuid(),
                Code = s.Code.Trim(),
                Description = s.Description.Trim(),
            };

            scope.RaiseCreated();
            _db.Scopes.Add(scope);
        }

        await _db.SaveChangesAsync(ct);
    }

    private static IReadOnlyList<(string Code, string Description)> GetDesiredScopes() =>
        [
            ("users.read", "View users"),
            ("users.create", "Create users"),
            ("users.update", "Update users"),
            ("users.delete", "Delete users"),
            ("roles.read", "View roles"),
            ("roles.create", "Create roles"),
            ("roles.update", "Update roles"),
            ("roles.delete", "Delete roles"),
            ("roles.scopes.assign", "Assign/remove scopes to roles"),
            ("users.roles.assign", "Assign/remove roles to users"),
            ("branches.read", "View branches"),
            ("branches.create", "Create branches"),
            ("branches.update", "Update branches"),
            ("branches.delete", "Delete branches"),
            ("warehouses.read", "View warehouses"),
            ("warehouses.create", "Create warehouses"),
            ("warehouses.update", "Update warehouses"),
            ("warehouses.delete", "Delete warehouses"),
            ("taxes.read", "View taxes"),
            ("taxes.create", "Create taxes"),
            ("taxes.update", "Update taxes"),
            ("taxes.delete", "Delete taxes"),
            ("uoms.read", "View units of measure"),
            ("uoms.create", "Create units of measure"),
            ("uoms.update", "Update units of measure"),
            ("uoms.delete", "Delete units of measure"),
            ("categories.read", "View product categories"),
            ("categories.create", "Create product categories"),
            ("categories.update", "Update product categories"),
            ("categories.delete", "Delete product categories"),
            ("products.read", "View products"),
            ("products.create", "Create products"),
            ("products.update", "Update products"),
            ("products.delete", "Delete products"),
            ("suppliers.read", "View suppliers"),
            ("suppliers.create", "Create suppliers"),
            ("suppliers.update", "Update suppliers"),
            ("suppliers.delete", "Delete suppliers"),
            ("suppliers.credit.assign", "Assign/update supplier credit conditions"),
            ("clients.read", "View clients"),
            ("clients.create", "Create clients"),
            ("clients.update", "Update clients"),
            ("clients.delete", "Delete clients"),
            ("employees.read", "View employees"),
            ("employees.create", "Create employees"),
            ("employees.update", "Update employees"),
            ("employees.delete", "Delete employees"),
            ("employees.attendance.checkin", "Employee check-in"),
            ("employees.attendance.checkout", "Employee check-out"),
            ("inventory.movements.create", "Register inventory movement"),
            ("inventory.transfers.read", "View inventory transfers"),
            ("inventory.transfers.create", "Create inventory transfer"),
            ("inventory.transfers.update", "Update inventory transfer"),
            ("inventory.transfers.delete", "Delete inventory transfer"),
            ("inventory.transfers.request_review", "Request review for inventory transfer"),
            ("inventory.transfers.approve", "Approve inventory transfer"),
            ("inventory.transfers.reject", "Reject inventory transfer"),
            ("inventory.transfers.confirm", "Confirm inventory transfer"),
            ("inventory.transfers.cancel", "Cancel inventory transfer"),
            ("inventory.by_branch.read", "View inventory by branch"),
            ("inventory.stock.read", "View consolidated stock"),
            ("auth.login", "Login"),
            ("security.scopes.read", "List scopes"),
            ("security.admin.reset_password", "Reset admin password"),
            ("roles.system.protect", "Protect system roles"),
        ];
}
