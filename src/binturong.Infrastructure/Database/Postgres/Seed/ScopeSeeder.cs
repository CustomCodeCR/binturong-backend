using Application.Security;
using Domain.Scopes;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Postgres.Seed;

public sealed class ScopeSeeder
{
    private readonly ApplicationDbContext _db;

    public ScopeSeeder(ApplicationDbContext db) => _db = db;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var desired = ScopeRegistry.All;

        var existingCodes = await _db.Scopes.AsNoTracking().Select(x => x.Code).ToListAsync(ct);

        var existing = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var s in desired)
        {
            if (existing.Contains(s.Code))
                continue;

            var scope = new Scope
            {
                Id = Guid.NewGuid(),
                Code = s.Code,
                Description = s.Description,
            };

            scope.RaiseCreated();
            _db.Scopes.Add(scope);
        }

        await _db.SaveChangesAsync(ct);
    }
}
