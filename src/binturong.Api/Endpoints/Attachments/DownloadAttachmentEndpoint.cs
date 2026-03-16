using Application.Abstractions.Messaging;
using Application.Features.Attachments.Download;

namespace Api.Endpoints.Attachments;

public sealed class AttachmentsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/attachments").WithTags("Attachments");

        group.MapGet(
            "/{module}/{attachmentId:guid}/download",
            async (
                string module,
                Guid attachmentId,
                IQueryHandler<DownloadAttachmentQuery, DownloadAttachmentResponse> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(
                    new DownloadAttachmentQuery(module, attachmentId),
                    ct
                );

                if (result.IsFailure)
                {
                    if (
                        string.Equals(
                            result.Error.Code,
                            "Attachments.NotFound",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        return Results.NotFound(result.Error);
                    }

                    return Results.BadRequest(result.Error);
                }

                return Results.Ok(result.Value);
            }
        );
    }
}
