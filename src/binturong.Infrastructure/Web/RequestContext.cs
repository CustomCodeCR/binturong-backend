using Application.Abstractions.Web;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Web;

internal sealed class RequestContext : IRequestContext
{
    private readonly IHttpContextAccessor _http;

    public RequestContext(IHttpContextAccessor http) => _http = http;

    public string IpAddress =>
        _http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

    public string UserAgent =>
        _http.HttpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty;
}
