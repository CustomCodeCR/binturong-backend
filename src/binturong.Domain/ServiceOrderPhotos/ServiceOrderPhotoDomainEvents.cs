using SharedKernel;

namespace Domain.ServiceOrderPhotos;

public sealed record ServiceOrderPhotoUploadedDomainEvent(Guid PhotoId) : IDomainEvent;
