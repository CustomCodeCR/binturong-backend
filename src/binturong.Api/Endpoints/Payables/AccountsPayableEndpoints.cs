using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Payables.AccountsPayable.GetAccountsPayableById;
using Application.Features.Payables.AccountsPayable.GetAccountsPayables;
using Application.Features.Payables.AccountsPayable.RegisterPayment;
using Application.ReadModels.Payables;
using Application.Security.Scopes;

namespace Api.Endpoints.Payables;

public sealed class AccountsPayableEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payables/accounts-payable")
            .WithTags("Payables - Accounts Payable");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    bool? overdue,
                    string? search,
                    IQueryHandler<
                        GetAccountsPayablesQuery,
                        IReadOnlyList<AccountsPayableReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetAccountsPayablesQuery(page ?? 1, pageSize ?? 50, overdue, search),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.AccountsPayableRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetAccountsPayableByIdQuery, AccountsPayableReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetAccountsPayableByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.AccountsPayableRead);

        group
            .MapPost(
                "/{id:guid}/payments",
                async (
                    Guid id,
                    RegisterAccountsPayablePaymentRequest req,
                    ICommandHandler<RegisterAccountsPayablePaymentCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RegisterAccountsPayablePaymentCommand(
                            id,
                            req.Amount,
                            req.PaidAtUtc,
                            req.Notes
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.AccountsPayablePaymentsCreate);
    }
}
