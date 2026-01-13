using SharedKernel;

namespace Domain.SupplierAttachments;

public sealed record SupplierAttachmentUploadedDomainEvent(Guid AttachmentId) : IDomainEvent;
