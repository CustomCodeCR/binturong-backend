using SharedKernel;

namespace Domain.ClientAttachments;

public sealed record ClientAttachmentUploadedDomainEvent(Guid AttachmentId) : IDomainEvent;
