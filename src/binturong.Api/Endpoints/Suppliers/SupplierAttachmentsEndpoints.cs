using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Suppliers.Attachments.Remove;
using Application.Features.Suppliers.Attachments.Upload;
using Application.Security.Scopes;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Suppliers;

public sealed class SupplierAttachmentsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/suppliers/{supplierId:guid}/attachments")
            .WithTags("SupplierAttachments");

        // POST /api/suppliers/{supplierId}/attachments (multipart/form-data)
        group
            .MapPost(
                "/",
                async (
                    Guid supplierId,
                    [FromForm] SupplierAttachmentUploadForm form,
                    ICommandHandler<UploadSupplierAttachmentCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    if (form.File is null || form.File.Length <= 0)
                        return Results.BadRequest(
                            SharedKernel.Error.Validation(
                                "Suppliers.Attachments.Missing",
                                "File is required."
                            )
                        );

                    if (string.IsNullOrWhiteSpace(form.DocumentType))
                        return Results.BadRequest(
                            SharedKernel.Error.Validation(
                                "Suppliers.Attachments.DocumentTypeRequired",
                                "DocumentType is required."
                            )
                        );

                    await using var stream = form.File.OpenReadStream();

                    var cmd = new UploadSupplierAttachmentCommand(
                        SupplierId: supplierId,
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
                            $"/api/suppliers/{supplierId}/attachments/{result.Value}",
                            new { attachmentId = result.Value }
                        );
                }
            )
            .DisableAntiforgery()
            .Accepts<SupplierAttachmentUploadForm>("multipart/form-data")
            .RequireScope(SecurityScopes.SuppliersUpdate);

        // DELETE /api/suppliers/{supplierId}/attachments/{attachmentId}
        group
            .MapDelete(
                "/{attachmentId:guid}",
                async (
                    Guid supplierId,
                    Guid attachmentId,
                    ICommandHandler<RemoveSupplierAttachmentCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RemoveSupplierAttachmentCommand(supplierId, attachmentId),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.SuppliersUpdate);
    }
}

// ✅ Swagger + binder friendly (multipart/form-data)
public sealed record SupplierAttachmentUploadForm(IFormFile File, string DocumentType);
