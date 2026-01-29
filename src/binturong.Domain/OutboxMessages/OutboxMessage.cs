using SharedKernel;

namespace Domain.OutboxMessages;

public sealed class OutboxMessage : Entity
{
    public Guid Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Type { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public string LastError { get; set; } = string.Empty;
    public DateTime? NextAttemptAt { get; set; }
    public DateTime? LockedUntil { get; set; }
}
