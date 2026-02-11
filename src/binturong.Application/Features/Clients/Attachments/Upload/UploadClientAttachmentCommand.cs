using Application.Abstractions.Messaging;

namespace Application.Features.Clients.Attachments.Upload;

public sealed record UploadClientAttachmentCommand(
    Guid ClientId,
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content,
    string DocumentType
) : ICommand<Guid>;
