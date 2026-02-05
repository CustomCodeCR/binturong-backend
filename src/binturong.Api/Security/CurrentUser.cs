using System.Security.Claims;
using Application.Abstractions.Security;

namespace Api.Security;

internal sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;

    public CurrentUser(IHttpContextAccessor http) => _http = http;

    public Guid UserId
    {
        get
        {
            var sub =
                _http.HttpContext?.User?.FindFirstValue("uid")
                ?? _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Name)
                ?? string.Empty;

            return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
        }
    }

    public IReadOnlyList<string> Scopes
    {
        get
        {
            var scp = _http.HttpContext?.User?.FindFirstValue("scp") ?? string.Empty;
            return scp.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
