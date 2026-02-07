using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Quotes.Accept;
using Application.Features.Quotes.AddDetail;
using Application.Features.Quotes.Create;
using Application.Features.Quotes.Expire;
using Application.Features.Quotes.GetQuoteById;
using Application.Features.Quotes.GetQuotes;
using Application.Features.Quotes.Reject;
using Application.Features.Quotes.Send;
using Application.ReadModels.Sales;
using Application.Security.Scopes;

namespace Api.Endpoints.Quotes;

public sealed class QuotesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quotes").WithTags("Quotes");

        // LIST
        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<GetQuotesQuery, IReadOnlyList<QuoteReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetQuotesQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.QuotesRead);

        // GET BY ID
        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetQuoteByIdQuery, QuoteReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetQuoteByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.QuotesRead);

        // CREATE
        group
            .MapPost(
                "/",
                async (
                    CreateQuoteRequest req,
                    ICommandHandler<CreateQuoteCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new CreateQuoteCommand(
                            req.Code,
                            req.ClientId,
                            req.BranchId,
                            req.IssueDate,
                            req.ValidUntil,
                            req.Currency,
                            req.ExchangeRate,
                            req.Notes
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/quotes/{result.Value}",
                            new { quoteId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.QuotesCreate);

        // ADD DETAIL
        group
            .MapPost(
                "/{id:guid}/details",
                async (
                    Guid id,
                    AddQuoteDetailRequest req,
                    ICommandHandler<AddQuoteDetailCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new AddQuoteDetailCommand(
                            id,
                            req.ProductId,
                            req.Quantity,
                            req.UnitPrice,
                            req.DiscountPerc,
                            req.TaxPerc
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/quotes/{id}",
                            new { quoteDetailId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.QuotesDetailsAdd);

        // SEND
        group
            .MapPost(
                "/{id:guid}/send",
                async (Guid id, ICommandHandler<SendQuoteCommand> handler, CancellationToken ct) =>
                {
                    var result = await handler.Handle(new SendQuoteCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.QuotesSend);

        // ACCEPT
        group
            .MapPost(
                "/{id:guid}/accept",
                async (
                    Guid id,
                    ICommandHandler<AcceptQuoteCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new AcceptQuoteCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.QuotesAccept);

        // REJECT
        group
            .MapPost(
                "/{id:guid}/reject",
                async (
                    Guid id,
                    RejectQuoteRequest req,
                    ICommandHandler<RejectQuoteCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new RejectQuoteCommand(id, req.Reason), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.QuotesReject);

        // EXPIRE
        group
            .MapPost(
                "/{id:guid}/expire",
                async (
                    Guid id,
                    ICommandHandler<ExpireQuoteCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new ExpireQuoteCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.QuotesExpire);
    }
}
