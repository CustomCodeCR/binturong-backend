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

        var admin = await _db.Users.FirstOrDefaultAsync(x => x.Username == "admin", ct);

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

        var adminRole =
            await _db.Roles.FirstOrDefaultAsync(x => x.Name == "SuperAdmin", ct)
            ?? await _db.Roles.FirstAsync(x => x.Name == "Admin", ct);

        var currentRoles = await _db.UserRoles.Where(x => x.UserId == admin.Id).ToListAsync(ct);

        foreach (var currentRole in currentRoles)
        {
            if (currentRole.RoleId != adminRole.Id)
            {
                _db.UserRoles.Remove(currentRole);
            }
        }

        var alreadyAssigned = currentRoles.Any(x => x.RoleId == adminRole.Id);

        if (!alreadyAssigned)
        {
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = admin.Id,
                RoleId = adminRole.Id,
            };

            userRole.RaiseAssigned();
            _db.UserRoles.Add(userRole);
        }

        await _db.SaveChangesAsync(ct);
    }
}
