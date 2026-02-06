using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Suppliers.Attachments.Remove;
using Application.Features.Suppliers.Attachments.Upload;
using Application.Security.Scopes;

namespace Api.Endpoints.Suppliers;

public sealed class SupplierAttachmentsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/suppliers/{supplierId:guid}/attachments")
            .WithTags("SupplierAttachments");

        group
            .MapPost(
                "/",
                async (
                    Guid supplierId,
                    UploadSupplierAttachmentRequest req,
                    ICommandHandler<UploadSupplierAttachmentCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new UploadSupplierAttachmentCommand(
                        supplierId,
                        req.FileName,
                        req.FileS3Key,
                        req.DocumentType
                    );

                    var result = await handler.Handle(cmd, ct);

                    if (result.IsFailure)
                        return Results.BadRequest(result.Error);

                    return Results.Created(
                        $"/api/suppliers/{supplierId}/attachments/{result.Value}",
                        new { attachmentId = result.Value }
                    );
                }
            )
            .RequireScope(SecurityScopes.SuppliersUpdate);

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
