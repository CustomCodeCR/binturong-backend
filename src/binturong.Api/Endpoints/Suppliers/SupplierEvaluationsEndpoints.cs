using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.SupplierEvaluations.Create;
using Application.Features.SupplierEvaluations.GetSupplierEvaluations;
using Application.ReadModels.CRM;
using Application.Security.Scopes;

namespace Api.Endpoints.Suppliers;

public sealed class SupplierEvaluationsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/suppliers/evaluations").WithTags("Suppliers - Evaluations");

        group
            .MapGet(
                "/{supplierId:guid}",
                async (
                    Guid supplierId,
                    int? page,
                    int? pageSize,
                    IQueryHandler<
                        GetSupplierEvaluationsQuery,
                        IReadOnlyList<SupplierEvaluationReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetSupplierEvaluationsQuery(supplierId, page ?? 1, pageSize ?? 50),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.SupplierEvaluationsRead);

        group
            .MapPost(
                "/",
                async (
                    CreateSupplierEvaluationRequest req,
                    ICommandHandler<CreateSupplierEvaluationCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreateSupplierEvaluationCommand(
                            req.SupplierId,
                            req.Score,
                            req.Comment,
                            req.EvaluatedAtUtc
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/suppliers/evaluations/{result.Value}",
                            new { supplierEvaluationId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.SupplierEvaluationsCreate);
    }
}
