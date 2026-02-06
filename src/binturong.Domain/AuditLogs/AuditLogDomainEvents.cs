using SharedKernel;

namespace Domain.AuditLogs;

public sealed record AuditLogCreatedDomainEvent(
    Guid AuditId,
    DateTime EventDate,
    Guid? UserId,
    string Module,
    string Entity,
    Guid? EntityId,
    string Action,
    string? DataBefore,
    string? DataAfter,
    string? Ip,
    string? UserAgent
) : IDomainEvent;
