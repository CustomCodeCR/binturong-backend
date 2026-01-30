using SharedKernel;

namespace Domain.ClientAttachments;

public sealed record ClientAttachmentUploadedDomainEvent(
    Guid ClientId,
    Guid AttachmentId,
    string FileName,
    string FileS3Key,
    string DocumentType,
    DateTime UploadedAt,
    DateTime UpdatedAt
) : IDomainEvent;

public sealed record ClientAttachmentDeletedDomainEvent(Guid ClientId, Guid AttachmentId)
    : IDomainEvent;
