using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Audit.GetAuditLogs;
using Application.ReadModels.Audit;
using Application.Security.Scopes;

namespace Api.Endpoints.Audit;

public sealed class AuditEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/audit").WithTags("Audit");

        group
            .MapGet(
                "/",
                async (
                    DateTime? from,
                    DateTime? to,
                    string? module,
                    string? action,
                    IQueryHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var query = new GetAuditLogsQuery(from, to, null, module, action);
                    var result = await handler.Handle(query, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.AuditRead);
    }
}
