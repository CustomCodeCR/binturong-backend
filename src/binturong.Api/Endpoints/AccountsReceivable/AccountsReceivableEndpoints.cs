using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.AccountsReceivable.GetAccountsReceivableStatus;
using Application.ReadModels.Sales;
using Application.Security.Scopes;

namespace Api.Endpoints.AccountsReceivable;

public sealed class AccountsReceivableEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts-receivable").WithTags("AccountsReceivable");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    Guid? clientId,
                    string? status,
                    IQueryHandler<
                        GetAccountsReceivableStatusQuery,
                        IReadOnlyList<InvoiceReadModel>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetAccountsReceivableStatusQuery(
                            page ?? 1,
                            pageSize ?? 50,
                            clientId,
                            status
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.AccountsReceivableRead);
    }
}
