using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Dashboard.GetDashboard;
using Application.ReadModels.Dashboard;
using Application.Security.Scopes;

namespace Api.Endpoints.Dashboard;

public sealed class DashboardEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard");

        group
            .MapGet(
                "/",
                async (
                    Guid? branchId,
                    IQueryHandler<GetDashboardQuery, DashboardReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetDashboardQuery(branchId), ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.DashboardRead);
    }
}
