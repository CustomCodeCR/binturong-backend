using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Branches.Create;
using Application.Features.Branches.Delete;
using Application.Features.Branches.GetBranchById;
using Application.Features.Branches.GetBranches;
using Application.Features.Branches.Reports.ExportBranchSalesReportExcel;
using Application.Features.Branches.Reports.ExportBranchSalesReportPdf;
using Application.Features.Branches.Reports.GetBranchComparisonReport;
using Application.Features.Branches.Reports.GetBranchSalesReport;
using Application.Features.Branches.Update;
using Application.ReadModels.MasterData;
using Application.ReadModels.Reports;
using Application.Security.Scopes;

namespace Api.Endpoints.Branches;

public sealed class BranchesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/branches").WithTags("Branches");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<GetBranchesQuery, IReadOnlyList<BranchReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetBranchesQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.BranchesRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetBranchByIdQuery, BranchReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetBranchByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.BranchesRead);

        group
            .MapGet(
                "/{id:guid}/reports/sales",
                async (
                    Guid id,
                    DateTime? from,
                    DateTime? to,
                    string? status,
                    IQueryHandler<GetBranchSalesReportQuery, BranchSalesReportReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetBranchSalesReportQuery(id, from, to, status),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.BranchesRead);

        group
            .MapGet(
                "/reports/compare",
                async (
                    Guid branchAId,
                    Guid branchBId,
                    DateTime? from,
                    DateTime? to,
                    string? status,
                    IQueryHandler<
                        GetBranchComparisonReportQuery,
                        BranchComparisonReportReadModel
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetBranchComparisonReportQuery(branchAId, branchBId, from, to, status),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.BranchesRead);

        group
            .MapGet(
                "/{id:guid}/reports/sales/pdf",
                async (
                    Guid id,
                    DateTime? from,
                    DateTime? to,
                    string? status,
                    ICommandHandler<ExportBranchSalesReportPdfCommand, byte[]> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportBranchSalesReportPdfCommand(id, from, to, status),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value,
                            "application/pdf",
                            $"branch_sales_report_{id:N}.pdf"
                        );
                }
            )
            .RequireScope(SecurityScopes.BranchesRead);

        group
            .MapGet(
                "/{id:guid}/reports/sales/excel",
                async (
                    Guid id,
                    DateTime? from,
                    DateTime? to,
                    string? status,
                    ICommandHandler<ExportBranchSalesReportExcelCommand, byte[]> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportBranchSalesReportExcelCommand(id, from, to, status),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"branch_sales_report_{id:N}.xlsx"
                        );
                }
            )
            .RequireScope(SecurityScopes.BranchesRead);

        group
            .MapPost(
                "/",
                async (
                    CreateBranchRequest req,
                    ICommandHandler<CreateBranchCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreateBranchCommand(
                            req.Code,
                            req.Name,
                            req.Address,
                            req.Phone,
                            req.IsActive
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/branches/{result.Value}",
                            new { branchId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.BranchesCreate);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    UpdateBranchRequest req,
                    ICommandHandler<UpdateBranchCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new UpdateBranchCommand(
                            id,
                            req.Code,
                            req.Name,
                            req.Address,
                            req.Phone,
                            req.IsActive
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.BranchesUpdate);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    ICommandHandler<DeleteBranchCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new DeleteBranchCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.BranchesDelete);
    }
}
