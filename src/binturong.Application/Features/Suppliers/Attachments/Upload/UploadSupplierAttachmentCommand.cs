using Application.Abstractions.Messaging;

namespace Application.Features.Suppliers.Attachments.Upload;

public sealed record UploadSupplierAttachmentCommand(
    Guid SupplierId,
    string FileName,
    string FileS3Key,
    string DocumentType
) : ICommand<Guid>;
