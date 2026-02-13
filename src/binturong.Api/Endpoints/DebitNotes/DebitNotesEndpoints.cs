using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.DebitNotes.Create;
using Application.Features.DebitNotes.Delete;
using Application.Features.DebitNotes.Emit;
using Application.Features.DebitNotes.GetDebitNoteById;
using Application.Features.DebitNotes.GetDebitNotes;
using Application.ReadModels.Sales;
using Application.Security.Scopes;

namespace Api.Endpoints.DebitNotes;

public sealed class DebitNotesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/debit-notes").WithTags("DebitNotes");

        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<GetDebitNotesQuery, IReadOnlyList<DebitNoteReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetDebitNotesQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.DebitNotesRead);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetDebitNoteByIdQuery, DebitNoteReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetDebitNoteByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.DebitNotesRead);

        group
            .MapPost(
                "/",
                async (
                    CreateDebitNoteRequest req,
                    ICommandHandler<CreateDebitNoteCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    // FIRMA REAL (la que pegaste):
                    // CreateDebitNoteCommand(Guid InvoiceId, DateTime IssueDate, string Reason, decimal TotalAmount)
                    var result = await handler.Handle(
                        new CreateDebitNoteCommand(
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
                            $"/api/debit-notes/{result.Value}",
                            new { debitNoteId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.DebitNotesCreate);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    ICommandHandler<DeleteDebitNoteCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new DeleteDebitNoteCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.DebitNotesDelete);

        group
            .MapPost(
                "/{id:guid}/emit",
                async (
                    Guid id,
                    ICommandHandler<EmitDebitNoteCommand, EmitDebitNoteResponse> handler,
                    CancellationToken ct
                ) =>
                {
                    // EmitDebitNoteCommand(Guid DebitNoteId)
                    var result = await handler.Handle(new EmitDebitNoteCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.DebitNotesEmit);
    }
}
