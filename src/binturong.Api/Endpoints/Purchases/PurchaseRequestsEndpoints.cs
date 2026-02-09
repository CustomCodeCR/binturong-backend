using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Purchases.PurchaseRequests.Create;
using Application.Features.Purchases.PurchaseRequests.GetPurchaseRequestById;
using Application.Features.Purchases.PurchaseRequests.GetPurchaseRequests;
using Application.ReadModels.Purchases;
using Application.Security.Scopes;

namespace Api.Endpoints.Purchases;

public sealed class PurchaseRequestsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/purchases/requests").WithTags("Purchases - Requests");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<
                        GetPurchaseRequestsQuery,
                        IReadOnlyList<PurchaseRequestReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetPurchaseRequestsQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PurchaseRequestsRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetPurchaseRequestByIdQuery, PurchaseRequestReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetPurchaseRequestByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PurchaseRequestsRead);

        group
            .MapPost(
                "/",
                async (
                    CreatePurchaseRequestRequest req,
                    ICommandHandler<CreatePurchaseRequestCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreatePurchaseRequestCommand(
                            req.Code,
                            req.BranchId,
                            req.RequestedById,
                            req.RequestDateUtc,
                            req.Notes
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/purchases/requests/{result.Value}",
                            new { requestId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.PurchaseRequestsCreate);
    }
}
