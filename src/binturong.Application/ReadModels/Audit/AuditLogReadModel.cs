namespace Application.ReadModels.Audit;

public sealed class AuditLogReadModel
{
    public string Id { get; init; } = default!; // "audit:{AuditId}"
    public int AuditId { get; init; }

    public DateTime EventDate { get; init; }
    public int? UserId { get; init; }

    public string Module { get; init; } = default!;
    public string Entity { get; init; } = default!;
    public int? EntityId { get; init; }

    public string Action { get; init; } = default!;
    public string? DataBefore { get; init; }
    public string? DataAfter { get; init; }

    public string? Ip { get; init; }
    public string? UserAgent { get; init; }
}
