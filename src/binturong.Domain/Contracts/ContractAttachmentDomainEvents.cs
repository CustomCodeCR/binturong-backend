using SharedKernel;

namespace Domain.Contracts;

public sealed record ContractAttachmentUploadedDomainEvent(
    Guid ContractId,
    Guid AttachmentId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string StoragePath,
    DateTime UploadedAtUtc,
    Guid UploadedByUserId
) : IDomainEvent;

public sealed record ContractAttachmentDeletedDomainEvent(Guid ContractId, Guid AttachmentId)
    : IDomainEvent;
