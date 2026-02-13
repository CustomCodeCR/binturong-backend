using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.CreditNotes.Create;
using Application.Features.CreditNotes.Delete;
using Application.Features.CreditNotes.Emit;
using Application.Features.CreditNotes.GetCreditNoteById;
using Application.Features.CreditNotes.GetCreditNotes;
using Application.ReadModels.Sales;
using Application.Security.Scopes;

namespace Api.Endpoints.CreditNotes;

public sealed class CreditNotesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/credit-notes").WithTags("CreditNotes");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<GetCreditNotesQuery, IReadOnlyList<CreditNoteReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetCreditNotesQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.CreditNotesRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetCreditNoteByIdQuery, CreditNoteReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetCreditNoteByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.CreditNotesRead);

        group
            .MapPost(
                "/",
                async (
                    CreateCreditNoteRequest req,
                    ICommandHandler<CreateCreditNoteCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    // FIRMA REAL (la que pegaste):
                    // CreateCreditNoteCommand(Guid InvoiceId, DateTime IssueDate, string Reason, decimal TotalAmount)
                    var result = await handler.Handle(
                        new CreateCreditNoteCommand(
                            req.InvoiceId,
                            req.IssueDate,
                            req.Reason,
                            req.TotalAmount
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/credit-notes/{result.Value}",
                            new { creditNoteId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.CreditNotesCreate);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    ICommandHandler<DeleteCreditNoteCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new DeleteCreditNoteCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.CreditNotesDelete);

        group
            .MapPost(
                "/{id:guid}/emit",
                async (
                    Guid id,
                    ICommandHandler<EmitCreditNoteCommand, EmitCreditNoteResponse> handler,
                    CancellationToken ct
                ) =>
                {
                    // EmitCreditNoteCommand(Guid CreditNoteId)
                    var result = await handler.Handle(new EmitCreditNoteCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.CreditNotesEmit);
    }
}
