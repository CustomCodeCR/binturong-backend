using Application.Abstractions.Messaging;

namespace Application.Features.Suppliers.Attachments.Upload;

public sealed record UploadSupplierAttachmentCommand(
    Guid SupplierId,
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content,
    string DocumentType
) : ICommand<Guid>;
