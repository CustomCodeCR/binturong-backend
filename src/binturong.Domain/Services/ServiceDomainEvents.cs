using SharedKernel;

namespace Domain.Services;

public sealed record ServiceCreatedDomainEvent(Guid ServiceId) : IDomainEvent;
