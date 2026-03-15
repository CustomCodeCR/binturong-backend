using Api.Security;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Features.Security.GetScopes;
using Application.ReadModels.Security;
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
                async (
                    string? search,
                    IQueryHandler<GetScopesQuery, IReadOnlyList<ScopeCatalogReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var query = new GetScopesQuery(search);

                    var result = await handler.Handle(query, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
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
