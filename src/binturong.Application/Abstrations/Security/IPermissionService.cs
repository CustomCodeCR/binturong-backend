namespace Application.Abstractions.Security;

public interface IPermissionService
{
    Task<bool> HasScopeAsync(Guid userId, string scope, CancellationToken ct);

    Task<IReadOnlyList<string>> GetUserScopesAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<string>> GetUserRoleNamesAsync(Guid userId, CancellationToken ct);

    Task<IReadOnlyList<string>> GetAllScopesAsync(CancellationToken ct);
}
