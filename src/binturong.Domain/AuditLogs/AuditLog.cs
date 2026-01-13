using SharedKernel;

namespace Domain.AuditLogs;

public sealed class AuditLog : Entity
{
    public Guid Id { get; set; }
    public DateTime EventDate { get; set; }
    public Guid? UserId { get; set; }
    public string Module { get; set; } = string.Empty;

    // Column name in DB: "Entity"
    public string EntityName { get; set; } = string.Empty;

    public int? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string DataBefore { get; set; } = string.Empty;
    public string DataAfter { get; set; } = string.Empty;
    public string IP { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;

    public Domain.Users.User? User { get; set; }
}
