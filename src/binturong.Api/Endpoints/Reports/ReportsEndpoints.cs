using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Reports.CreateSchedule;
using Application.Features.Reports.ExportClientReportExcel;
using Application.Features.Reports.ExportFinancialReportPdf;
using Application.Features.Reports.ExportInventoryReportExcel;
using Application.Features.Reports.ExportServiceOrdersReportExcel;
using Application.Features.Reports.GetClientReport;
using Application.Features.Reports.GetFinancialReport;
using Application.Features.Reports.GetInventoryReport;
using Application.Features.Reports.GetSchedules;
using Application.Features.Reports.GetServiceOrdersReport;
using Application.Features.Reports.UpdateSchedule;
using Application.ReadModels.Reports;
using Application.Security.Scopes;

namespace Api.Endpoints.Reports;

public sealed class ReportsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports").WithTags("Reports");

        group
            .MapGet(
                "/financial",
                async (
                    DateTime fromUtc,
                    DateTime toUtc,
                    IQueryHandler<GetFinancialReportQuery, FinancialReportReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetFinancialReportQuery(fromUtc, toUtc),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ReportsFinancialRead);

        group
            .MapGet(
                "/financial/export-pdf",
                async (
                    DateTime fromUtc,
                    DateTime toUtc,
                    IQueryHandler<
                        ExportFinancialReportPdfQuery,
                        ExportFinancialReportPdfResponse
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportFinancialReportPdfQuery(fromUtc, toUtc),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value.Content,
                            result.Value.ContentType,
                            result.Value.FileName
                        );
                }
            )
            .RequireScope(SecurityScopes.ReportsFinancialExportPdf);

        group
            .MapGet(
                "/inventory",
                async (
                    Guid? categoryId,
                    IQueryHandler<GetInventoryReportQuery, InventoryReportReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetInventoryReportQuery(categoryId), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ReportsInventoryRead);

        group
            .MapGet(
                "/inventory/export-excel",
                async (
                    Guid? categoryId,
                    IQueryHandler<
                        ExportInventoryReportExcelQuery,
                        ExportInventoryReportExcelResponse
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportInventoryReportExcelQuery(categoryId),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value.Content,
                            result.Value.ContentType,
                            result.Value.FileName
                        );
                }
            )
            .RequireScope(SecurityScopes.ReportsInventoryExportExcel);

        group
            .MapGet(
                "/clients/{clientId:guid}",
                async (
                    Guid clientId,
                    IQueryHandler<GetClientReportQuery, ClientReportReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetClientReportQuery(clientId), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ReportsClientsRead);

        group
            .MapGet(
                "/clients/{clientId:guid}/export-excel",
                async (
                    Guid clientId,
                    IQueryHandler<
                        ExportClientReportExcelQuery,
                        ExportClientReportExcelResponse
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportClientReportExcelQuery(clientId),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value.Content,
                            result.Value.ContentType,
                            result.Value.FileName
                        );
                }
            )
            .RequireScope(SecurityScopes.ReportsClientsExport);

        group
            .MapGet(
                "/service-orders",
                async (
                    DateTime fromUtc,
                    DateTime toUtc,
                    Guid? employeeId,
                    IQueryHandler<
                        GetServiceOrdersReportQuery,
                        ServiceOrdersReportReadModel
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetServiceOrdersReportQuery(fromUtc, toUtc, employeeId),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ReportsServiceOrdersRead);

        group
            .MapGet(
                "/service-orders/export-excel",
                async (
                    DateTime fromUtc,
                    DateTime toUtc,
                    Guid? employeeId,
                    IQueryHandler<
                        ExportServiceOrdersReportExcelQuery,
                        ExportServiceOrdersReportExcelResponse
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportServiceOrdersReportExcelQuery(fromUtc, toUtc, employeeId),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value.Content,
                            result.Value.ContentType,
                            result.Value.FileName
                        );
                }
            )
            .RequireScope(SecurityScopes.ReportsServiceOrdersExport);

        group
            .MapGet(
                "/schedules",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<
                        GetReportSchedulesQuery,
                        IReadOnlyList<ReportScheduleReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetReportSchedulesQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ReportsSchedulesRead);

        group
            .MapPost(
                "/schedules",
                async (
                    CreateReportScheduleRequest req,
                    ICommandHandler<CreateReportScheduleCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreateReportScheduleCommand(
                            req.Name,
                            req.ReportType,
                            req.Frequency,
                            req.RecipientEmail,
                            req.TimeOfDayUtc,
                            req.IsActive,
                            req.BranchId,
                            req.CategoryId,
                            req.ClientId,
                            req.EmployeeId
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            "/api/reports/schedules",
                            new { reportScheduleId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.ReportsSchedulesCreate);

        group
            .MapPut(
                "/schedules/{id:guid}",
                async (
                    Guid id,
                    UpdateReportScheduleRequest req,
                    ICommandHandler<UpdateReportScheduleCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new UpdateReportScheduleCommand(
                            id,
                            req.Name,
                            req.ReportType,
                            req.Frequency,
                            req.RecipientEmail,
                            req.TimeOfDayUtc,
                            req.IsActive,
                            req.BranchId,
                            req.CategoryId,
                            req.ClientId,
                            req.EmployeeId
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.ReportsSchedulesUpdate);
    }
}
