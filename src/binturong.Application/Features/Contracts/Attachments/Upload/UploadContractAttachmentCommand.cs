using Application.Abstractions.Messaging;

namespace Application.Features.Contracts.Attachments.Upload;

public sealed record UploadContractAttachmentCommand(
    Guid ContractId,
    string? FileName,
    string? ContentType,
    long SizeBytes,
    Stream Content
) : ICommand<Guid>;
