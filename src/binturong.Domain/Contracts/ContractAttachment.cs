using SharedKernel;

namespace Domain.Contracts;

public sealed class ContractAttachment : Entity
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }

    public string FileName { get; set; } = string.Empty; // original name
    public string ContentType { get; set; } = string.Empty; // "application/pdf"
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty; // relative path under uploads root
    public string StorageKey { get; set; } = string.Empty;

    public DateTime UploadedAtUtc { get; set; }
    public Guid UploadedByUserId { get; set; }

    public void RaiseUploaded() =>
        Raise(
            new ContractAttachmentUploadedDomainEvent(
                ContractId,
                Id,
                FileName,
                ContentType,
                SizeBytes,
                StoragePath,
                UploadedAtUtc,
                UploadedByUserId
            )
        );

    public void RaiseDeleted() => Raise(new ContractAttachmentDeletedDomainEvent(ContractId, Id));
}
