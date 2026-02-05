using Application.Abstractions.Security;

namespace Api.Security;

public static class ScopeAuthorizationExtensions
{
    public static RouteHandlerBuilder RequireScope(this RouteHandlerBuilder b, string scope) =>
        b.AddEndpointFilter(
            async (ctx, next) =>
            {
                var perm = ctx.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
                var current = ctx.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();

                if (current.UserId == Guid.Empty)
                    return Results.Unauthorized();

                var ok = await perm.HasScopeAsync(
                    current.UserId,
                    scope,
                    ctx.HttpContext.RequestAborted
                );

                if (!ok)
                    return Results.Forbid();

                return await next(ctx);
            }
        );
}
