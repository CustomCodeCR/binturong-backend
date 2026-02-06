using SharedKernel;

namespace Domain.AuditLogs;

public sealed class AuditLog : Entity
{
    public Guid Id { get; set; }
    public DateTime EventDate { get; set; }
    public Guid? UserId { get; set; }
    public string Module { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string DataBefore { get; set; } = string.Empty;
    public string DataAfter { get; set; } = string.Empty;
    public string IP { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;

    public Domain.Users.User? User { get; set; }

    public static AuditLog Create(
        Guid? userId,
        string module,
        string entity,
        Guid? entityId,
        string action,
        string? before,
        string? after,
        string? ip,
        string? userAgent
    )
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            EventDate = DateTime.UtcNow,
            UserId = userId,
            Module = module,
            EntityName = entity,
            EntityId = entityId,
            Action = action,
            DataBefore = before ?? string.Empty,
            DataAfter = after ?? string.Empty,
            IP = ip ?? string.Empty,
            UserAgent = userAgent ?? string.Empty,
        };

        log.Raise(
            new AuditLogCreatedDomainEvent(
                log.Id,
                log.EventDate,
                log.UserId,
                log.Module,
                log.EntityName,
                log.EntityId,
                log.Action,
                log.DataBefore,
                log.DataAfter,
                log.IP,
                log.UserAgent
            )
        );

        return log;
    }
}
