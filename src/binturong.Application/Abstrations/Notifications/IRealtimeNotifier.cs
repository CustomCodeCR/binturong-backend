namespace Application.Abstractions.Notifications;

public interface IRealtimeNotifier
{
    Task NotifyUserAsync(Guid userId, string type, object payload, CancellationToken ct);
    Task NotifyRoleAsync(string roleName, string type, object payload, CancellationToken ct);
}
