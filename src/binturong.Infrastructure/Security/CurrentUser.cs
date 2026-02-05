using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Abstractions.Security;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Security;

internal sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    public Guid UserId
    {
        get
        {
            var user = _accessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return Guid.Empty;

            // Prefer "uid", fallback to "sub"
            var raw =
                user.FindFirstValue("uid")
                ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
    }

    public IReadOnlyList<string> Scopes
    {
        get
        {
            var user = _accessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return Array.Empty<string>();

            // You generate "scp" as space-separated list
            var scp = user.FindFirstValue("scp");
            if (string.IsNullOrWhiteSpace(scp))
                return Array.Empty<string>();

            return scp.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
        }
    }
}
