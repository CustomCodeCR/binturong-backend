using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Clients.Attachments.Remove;
using Application.Features.Clients.Attachments.Upload;
using Application.Security.Scopes;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Clients;

public sealed class ClientAttachmentsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clients/{clientId:guid}/attachments")
            .WithTags("ClientAttachments");

        // POST /api/clients/{clientId}/attachments (multipart/form-data)
        group
            .MapPost(
                "/",
                async (
                    Guid clientId,
                    [FromForm] ClientAttachmentUploadForm form,
                    ICommandHandler<UploadClientAttachmentCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    if (form.File is null || form.File.Length <= 0)
                        return Results.BadRequest(
                            SharedKernel.Error.Validation(
                                "Clients.Attachments.Missing",
                                "File is required."
                            )
                        );

                    if (string.IsNullOrWhiteSpace(form.DocumentType))
                        return Results.BadRequest(
                            SharedKernel.Error.Validation(
                                "Clients.Attachments.DocumentTypeRequired",
                                "DocumentType is required."
                            )
                        );

                    await using var stream = form.File.OpenReadStream();

                    var cmd = new UploadClientAttachmentCommand(
                        ClientId: clientId,
                        FileName: form.File.FileName,
                        ContentType: string.IsNullOrWhiteSpace(form.File.ContentType)
                            ? "application/octet-stream"
                            : form.File.ContentType,
                        SizeBytes: form.File.Length,
                        Content: stream,
                        DocumentType: form.DocumentType.Trim()
                    );

                    var result = await handler.Handle(cmd, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/clients/{clientId}/attachments/{result.Value}",
                            new { attachmentId = result.Value }
                        );
                }
            )
            .DisableAntiforgery()
            .Accepts<ClientAttachmentUploadForm>("multipart/form-data")
            .RequireScope(SecurityScopes.ClientsUpdate);

        // DELETE /api/clients/{clientId}/attachments/{attachmentId}
        group
            .MapDelete(
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

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.ClientsUpdate);
    }
}

// ✅ multipart/form-data friendly
public sealed record ClientAttachmentUploadForm(IFormFile File, string DocumentType);
