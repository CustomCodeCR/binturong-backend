using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authorization;

public sealed class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options) { }

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // 1) Policies de scopes (dos formatos soportados)
        //    - "security.scopes.read"
        //    - "scope:security.scopes.read"
        if (policyName.StartsWith("scope:", StringComparison.OrdinalIgnoreCase))
        {
            var scope = policyName["scope:".Length..];

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("scope", scope)
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Si NO usás prefijo, pero te llega el scope directo como policy name:
        // "security.scopes.read"
        // (en tu caso EXACTAMENTE esto está pasando)
        if (LooksLikeScope(policyName))
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("scope", policyName)
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // 2) Dejá tu lógica existente de permisos (si ya la tenías)
        // Ejemplo: "perm:users.read" / "permission:..." etc.
        // Si ya tenés esta parte, mantenela aquí tal cual.

        return base.GetPolicyAsync(policyName);
    }

    private static bool LooksLikeScope(string policyName)
    {
        // Heurística simple: tus scopes son estilo "security.scopes.read"
        // (si querés, hacelo más estricto)
        return policyName.Contains('.', StringComparison.OrdinalIgnoreCase);
    }
}
