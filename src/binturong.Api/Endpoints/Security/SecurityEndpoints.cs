using Api.Security;
using Application.Abstractions.Security;
using Application.Security.Scopes;

namespace Api.Endpoints.Security;

public sealed class SecurityEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/security").WithTags("Security");

        group
            .MapGet(
                "/scopes",
                async (IPermissionService permissions, CancellationToken ct) =>
                {
                    var scopes = await permissions.GetAllScopesAsync(ct);
                    return Results.Ok(scopes);
                }
            )
            .RequireScope(SecurityScopes.SecurityScopesRead);

        group
            .MapPost(
                "/admin/reset-password",
                async (
                    ResetAdminPasswordRequest req,
                    IAdminPasswordResetService reset,
                    CancellationToken ct
                ) =>
                {
                    var result = await reset.ResetAdminPasswordAsync(req.NewPassword, ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.SecurityAdminResetPassword);
    }
}

public sealed record ResetAdminPasswordRequest(string NewPassword);
