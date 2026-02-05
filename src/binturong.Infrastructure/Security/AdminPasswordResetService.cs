using Application.Abstractions.Authentication;
using Application.Abstractions.Security;
using Infrastructure.Database.Postgres;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Security;

internal sealed class AdminPasswordResetService : IAdminPasswordResetService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;

    public AdminPasswordResetService(ApplicationDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<Result> ResetAdminPasswordAsync(string newPassword, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == "admin@system.local", ct);
        if (user is null)
            return Result.Failure(Error.NotFound("Admin.NotFound", "Admin user not found"));

        user.PasswordHash = _hasher.Hash(newPassword);
        user.MustChangePassword = true;
        user.FailedAttempts = 0;
        user.LockedUntil = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
