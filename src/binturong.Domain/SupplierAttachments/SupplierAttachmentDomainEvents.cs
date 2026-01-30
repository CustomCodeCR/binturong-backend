using SharedKernel;

namespace Domain.SupplierAttachments;

public sealed record SupplierAttachmentUploadedDomainEvent(
    Guid SupplierId,
    Guid AttachmentId,
    string FileName,
    string FileS3Key,
    string DocumentType,
    DateTime UploadedAt,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record SupplierAttachmentDeletedDomainEvent(Guid SupplierId, Guid AttachmentId)
    : IDomainEvent;
