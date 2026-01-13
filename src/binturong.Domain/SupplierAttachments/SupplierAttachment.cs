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

    public Domain.Suppliers.Supplier? Supplier { get; set; }
}
