using Application.Abstractions.Security;

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
            .RequireAuthorization("security.scopes.read");

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
            .RequireAuthorization("security.admin.reset_password");
    }
}

public sealed record ResetAdminPasswordRequest(string NewPassword);
