using Application.Abstractions.Messaging;

namespace Application.Features.Clients.Attachments.Upload;

public sealed record UploadClientAttachmentCommand(
    Guid ClientId,
    string FileName,
    string FileS3Key,
    string DocumentType
) : ICommand<Guid>;
