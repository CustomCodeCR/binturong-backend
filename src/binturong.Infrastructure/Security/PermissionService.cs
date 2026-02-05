using Application.Abstractions.Data;
using Application.Abstractions.Security;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Security;

internal sealed class PermissionService : IPermissionService
{
    private readonly IApplicationDbContext _db;

    public PermissionService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasScopeAsync(Guid userId, string scope, CancellationToken ct)
    {
        return await _db
            .UserRoles.Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role!.RoleScopes)
            .AnyAsync(rs => rs.Scope!.Code == scope, ct);
    }

    public async Task<IReadOnlyList<string>> GetUserScopesAsync(Guid userId, CancellationToken ct)
    {
        return await _db
            .UserRoles.Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role!.RoleScopes)
            .Select(rs => rs.Scope!.Code)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetUserRoleNamesAsync(
        Guid userId,
        CancellationToken ct
    )
    {
        return await _db
            .UserRoles.Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role!.Name)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetAllScopesAsync(CancellationToken ct)
    {
        return await _db
            .Scopes.Select(s => s.Code)
            .Distinct()
            .OrderBy(code => code)
            .ToListAsync(ct);
    }
}
