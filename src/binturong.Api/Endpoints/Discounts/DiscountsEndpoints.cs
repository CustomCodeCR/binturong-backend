using Api.Security;
using Application.Abstractions.Messaging;
using Application.Common.Selects;
using Application.Features.Discounts.ApplyGlobal;
using Application.Features.Discounts.ApplyGlobalApproved;
using Application.Features.Discounts.ApplyLine;
using Application.Features.Discounts.ApplyLineApproved;
using Application.Features.Discounts.Approve;
using Application.Features.Discounts.CreatePolicy;
using Application.Features.Discounts.ExportHistory;
using Application.Features.Discounts.GetApprovalRequests;
using Application.Features.Discounts.GetDiscountHistory;
using Application.Features.Discounts.GetPolicies;
using Application.Features.Discounts.GetPoliciesSelect;
using Application.Features.Discounts.GetPolicyById;
using Application.Features.Discounts.Reject;
using Application.Features.Discounts.RemoveGlobal;
using Application.Features.Discounts.RemoveLine;
using Application.Features.Discounts.RequestGlobalApproval;
using Application.Features.Discounts.RequestLineApproval;
using Application.Features.Discounts.UpdatePolicy;
using Application.ReadModels.Discounts;
using Application.Security.Scopes;

namespace Api.Endpoints.Discounts;

public sealed class DiscountsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/discounts").WithTags("Discounts");

        group
            .MapGet(
                "/policies",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<
                        GetDiscountPoliciesQuery,
                        IReadOnlyList<DiscountPolicyReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetDiscountPoliciesQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.DiscountsPoliciesRead);

        group
            .MapGet(
                "/policies/select",
                async (
                    string? search,
                    IQueryHandler<
                        GetDiscountPoliciesSelectQuery,
                        IReadOnlyList<SelectOptionDto>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetDiscountPoliciesSelectQuery(search),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.DiscountsPoliciesRead);

        group
            .MapGet(
                "/policies/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetDiscountPolicyByIdQuery, DiscountPolicyReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetDiscountPolicyByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.DiscountsPoliciesRead);

        group
            .MapPost(
                "/policies",
                async (
                    CreateDiscountPolicyRequest req,
                    ICommandHandler<CreateDiscountPolicyCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreateDiscountPolicyCommand(
                            req.Name,
                            req.MaxDiscountPercentage,
                            req.RequiresApprovalAboveLimit,
                            req.IsActive
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/discounts/policies/{result.Value}",
                            new { policyId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.DiscountsPoliciesCreate);

        group
            .MapPut(
                "/policies/{id:guid}",
                async (
                    Guid id,
                    UpdateDiscountPolicyRequest req,
                    ICommandHandler<UpdateDiscountPolicyCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new UpdateDiscountPolicyCommand(
                            id,
                            req.Name,
                            req.MaxDiscountPercentage,
                            req.RequiresApprovalAboveLimit,
                            req.IsActive
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.DiscountsPoliciesUpdate);

        group
            .MapGet(
                "/approval-requests",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    string? status,
                    IQueryHandler<
                        GetDiscountApprovalRequestsQuery,
                        IReadOnlyList<DiscountApprovalRequestReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetDiscountApprovalRequestsQuery(
                            page ?? 1,
                            pageSize ?? 50,
                            search,
                            status
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.DiscountsApprovalRead);

        group
            .MapPost(
                "/approval-requests/line",
                async (
                    RequestLineDiscountApprovalRequest req,
                    ICommandHandler<RequestLineDiscountApprovalCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RequestLineDiscountApprovalCommand(
                            req.SalesOrderId,
                            req.SalesOrderDetailId,
                            req.DiscountPerc,
                            req.Reason
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            "/api/discounts/approval-requests",
                            new { approvalRequestId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.DiscountsApprovalRequest);

        group
            .MapPost(
                "/approval-requests/global",
                async (
                    RequestGlobalDiscountApprovalRequest req,
                    ICommandHandler<RequestGlobalDiscountApprovalCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RequestGlobalDiscountApprovalCommand(
                            req.SalesOrderId,
                            req.DiscountPerc,
                            req.Reason
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            "/api/discounts/approval-requests",
                            new { approvalRequestId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.DiscountsApprovalRequest);

        group
            .MapPost(
                "/approval-requests/{id:guid}/approve",
                async (
                    Guid id,
                    ICommandHandler<ApproveDiscountApprovalRequestCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ApproveDiscountApprovalRequestCommand(id),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.DiscountsApprovalApprove);

        group
            .MapPost(
                "/approval-requests/{id:guid}/reject",
                async (
                    Guid id,
                    RejectDiscountApprovalRequest req,
                    ICommandHandler<RejectDiscountApprovalRequestCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RejectDiscountApprovalRequestCommand(id, req.RejectionReason),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.DiscountsApprovalReject);

        group
            .MapPost(
                "/sales-orders/line/apply",
                async (
                    ApplyLineDiscountRequest req,
                    ICommandHandler<ApplyLineDiscountCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ApplyLineDiscountCommand(
                            req.SalesOrderId,
                            req.SalesOrderDetailId,
                            req.DiscountPerc,
                            req.Reason,
                            req.PolicyId
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.DiscountsApply);

        group
            .MapPost(
                "/sales-orders/line/apply-approved",
                async (
                    ApplyApprovedLineDiscountRequest req,
                    ICommandHandler<ApplyApprovedLineDiscountCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ApplyApprovedLineDiscountCommand(
                            req.SalesOrderId,
                            req.SalesOrderDetailId,
                            req.DiscountPerc,
                            req.Reason,
                            req.ApprovalRequestId
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.DiscountsApply);

        group
            .MapPost(
                "/sales-orders/line/remove",
                async (
                    RemoveLineDiscountRequest req,
                    ICommandHandler<RemoveLineDiscountCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RemoveLineDiscountCommand(req.SalesOrderId, req.SalesOrderDetailId),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.DiscountsRemove);

        group
            .MapPost(
                "/sales-orders/global/apply",
                async (
                    ApplyGlobalDiscountRequest req,
                    ICommandHandler<ApplyGlobalDiscountCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ApplyGlobalDiscountCommand(
                            req.SalesOrderId,
                            req.DiscountPerc,
                            req.Reason,
                            req.PolicyId
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.DiscountsApply);

        group
            .MapPost(
                "/sales-orders/global/apply-approved",
                async (
                    ApplyApprovedGlobalDiscountRequest req,
                    ICommandHandler<ApplyApprovedGlobalDiscountCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ApplyApprovedGlobalDiscountCommand(
                            req.SalesOrderId,
                            req.DiscountPerc,
                            req.Reason,
                            req.ApprovalRequestId
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.DiscountsApply);

        group
            .MapPost(
                "/sales-orders/global/remove",
                async (
                    RemoveGlobalDiscountRequest req,
                    ICommandHandler<RemoveGlobalDiscountCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RemoveGlobalDiscountCommand(req.SalesOrderId),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.DiscountsRemove);

        group
            .MapGet(
                "/history",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    Guid? userId,
                    DateTime? fromUtc,
                    DateTime? toUtc,
                    IQueryHandler<
                        GetDiscountHistoryQuery,
                        IReadOnlyList<DiscountHistoryReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetDiscountHistoryQuery(
                            page ?? 1,
                            pageSize ?? 50,
                            search,
                            userId,
                            fromUtc,
                            toUtc
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.DiscountsHistoryRead);

        group
            .MapGet(
                "/history/export",
                async (
                    string? search,
                    Guid? userId,
                    DateTime? fromUtc,
                    DateTime? toUtc,
                    IQueryHandler<
                        ExportDiscountHistoryQuery,
                        ExportDiscountHistoryResponse
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportDiscountHistoryQuery(search, userId, fromUtc, toUtc),
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
            .RequireScope(SecurityScopes.DiscountsHistoryExport);
    }
}
