using Application.Abstractions.Messaging;
using Application.Features.Clients.Attachments.Remove;
using Application.Features.Clients.Attachments.Upload;

namespace Api.Endpoints.Clients;

public sealed class ClientAttachmentsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clients/{clientId:guid}/attachments")
            .WithTags("ClientAttachments");

        // =========================
        // UPLOAD (metadata)
        // POST /api/clients/{clientId}/attachments
        // =========================
        group.MapPost(
            "/",
            async (
                Guid clientId,
                UploadClientAttachmentRequest req,
                ICommandHandler<UploadClientAttachmentCommand, Guid> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new UploadClientAttachmentCommand(
                    clientId,
                    req.FileName,
                    req.FileS3Key,
                    req.DocumentType
                );

                var result = await handler.Handle(cmd, ct);

                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Created(
                    $"/api/clients/{clientId}/attachments/{result.Value}",
                    new { attachmentId = result.Value }
                );
            }
        );

        // =========================
        // DELETE
        // DELETE /api/clients/{clientId}/attachments/{attachmentId}
        // =========================
        group.MapDelete(
            "/{attachmentId:guid}",
            async (
                Guid clientId,
                Guid attachmentId,
                ICommandHandler<RemoveClientAttachmentCommand> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(
                    new RemoveClientAttachmentCommand(clientId, attachmentId),
                    ct
                );
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );
    }
}
