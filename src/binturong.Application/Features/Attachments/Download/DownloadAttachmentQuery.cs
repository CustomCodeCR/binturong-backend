using Application.Abstractions.Messaging;

namespace Application.Features.Attachments.Download;

public sealed record DownloadAttachmentQuery(string Module, Guid AttachmentId)
    : IQuery<DownloadAttachmentResponse>;

public sealed record DownloadAttachmentResponse(string FileName, string Url);
