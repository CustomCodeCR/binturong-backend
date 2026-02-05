using Application.Abstractions.Authentication;
using Domain.UserRoles;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Postgres.Seed;

public sealed class AdminUserSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public AdminUserSeeder(ApplicationDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // =========================
        // USER
        // =========================
        var admin = await _db.Users.FirstOrDefaultAsync(u => u.Username == "admin", ct);

        if (admin is null)
        {
            admin = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Email = "admin@system.local",
                PasswordHash = _passwordHasher.Hash("Admin123!"),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            };

            admin.RaiseRegistered();
            _db.Users.Add(admin);
            await _db.SaveChangesAsync(ct);
        }

        // =========================
        // ROLE
        // =========================
        var adminRole =
            await _db.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin", ct)
            ?? await _db.Roles.FirstAsync(r => r.Name == "Admin", ct);

        var existing = await _db.UserRoles.FirstOrDefaultAsync(
            x => x.UserId == admin.Id && x.RoleId == adminRole.Id,
            ct
        );

        if (existing is null)
        {
            var ur = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = admin.Id,
                RoleId = adminRole.Id,
            };

            ur.RaiseAssigned();

            _db.UserRoles.Add(ur);
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            existing.RaiseAssigned();
            await _db.SaveChangesAsync(ct);
        }
    }
}
