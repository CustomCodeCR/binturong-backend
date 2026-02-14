using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.PaymentMethods.Create;
using Application.Features.PaymentMethods.Delete;
using Application.Features.PaymentMethods.GetPaymentMethodById;
using Application.Features.PaymentMethods.GetPaymentMethods;
using Application.Features.PaymentMethods.Update;
using Application.ReadModels.MasterData;
using Application.Security.Scopes;

namespace Api.Endpoints.PaymentMethods;

public sealed class PaymentMethodsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payment-methods").WithTags("PaymentMethods");

        // GET list (Mongo)
        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<
                        GetPaymentMethodsQuery,
                        IReadOnlyList<PaymentMethodReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetPaymentMethodsQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PaymentMethodsRead);

        // GET by id (Mongo)
        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetPaymentMethodByIdQuery, PaymentMethodReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetPaymentMethodByIdQuery(id), ct);

                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PaymentMethodsRead);

        // CREATE (Write/Postgres)
        group
            .MapPost(
                "/",
                async (
                    CreatePaymentMethodRequest req,
                    ICommandHandler<CreatePaymentMethodCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreatePaymentMethodCommand(req.Code, req.Description, req.IsActive),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/payment-methods/{result.Value}",
                            new { paymentMethodId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.PaymentMethodsCreate);

        // UPDATE (Write/Postgres)
        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    UpdatePaymentMethodRequest req,
                    ICommandHandler<UpdatePaymentMethodCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new UpdatePaymentMethodCommand(id, req.Code, req.Description, req.IsActive),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.PaymentMethodsUpdate);

        // DELETE (Write/Postgres)
        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    ICommandHandler<DeletePaymentMethodCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new DeletePaymentMethodCommand(id), ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.PaymentMethodsDelete);
    }
}
