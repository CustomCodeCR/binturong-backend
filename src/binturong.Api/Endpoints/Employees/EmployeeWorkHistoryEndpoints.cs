using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Employees.ExportWorkHistory;
using Application.Features.Employees.GetWorkHistory;
using Application.ReadModels.Services;
using Application.Security.Scopes;

namespace Api.Endpoints.Employees;

public sealed class EmployeeWorkHistoryEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/employees").WithTags("Employees");

        group
            .MapGet(
                "/{id:guid}/work-history",
                async (
                    Guid id,
                    IQueryHandler<
                        GetEmployeeWorkHistoryQuery,
                        EmployeeWorkHistoryReadModel
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetEmployeeWorkHistoryQuery(id), ct);

                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.EmployeesWorkHistoryRead);

        group
            .MapGet(
                "/{id:guid}/work-history/export",
                async (
                    Guid id,
                    IQueryHandler<
                        ExportEmployeeWorkHistoryQuery,
                        ExportEmployeeWorkHistoryResponse
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new ExportEmployeeWorkHistoryQuery(id), ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value.Content,
                            result.Value.ContentType,
                            result.Value.FileName
                        );
                }
            )
            .RequireScope(SecurityScopes.EmployeesWorkHistoryExport);
    }
}
