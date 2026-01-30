using SharedKernel;

namespace Domain.SupplierAttachments;

public sealed class SupplierAttachment : Entity
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileS3Key { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Domain.Suppliers.Supplier? Supplier { get; set; }

    // =========================
    // Domain events
    // =========================

    public void RaiseUploaded() =>
        Raise(
            new SupplierAttachmentUploadedDomainEvent(
                SupplierId,
                Id,
                FileName,
                FileS3Key,
                DocumentType,
                UploadedAt,
                UpdatedAt
            )
        );

    public void RaiseDeleted() => Raise(new SupplierAttachmentDeletedDomainEvent(SupplierId, Id));
}
