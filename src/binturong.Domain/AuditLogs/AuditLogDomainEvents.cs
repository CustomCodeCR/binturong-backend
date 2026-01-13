using SharedKernel;

namespace Domain.AuditLogs;

public sealed record AuditLogCreatedDomainEvent(Guid AuditId) : IDomainEvent;
