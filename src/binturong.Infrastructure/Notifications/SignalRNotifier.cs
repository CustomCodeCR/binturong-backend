using Application.Abstractions.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Notifications;

public sealed class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationsHub> _hub;

    public SignalRNotifier(IHubContext<NotificationsHub> hub) => _hub = hub;

    public Task NotifyUserAsync(Guid userId, string type, object payload, CancellationToken ct) =>
        _hub.Clients.Group($"user:{userId:N}").SendAsync("notify", new { type, payload }, ct);

    public Task NotifyRoleAsync(
        string roleName,
        string type,
        object payload,
        CancellationToken ct
    ) => _hub.Clients.Group($"role:{roleName}").SendAsync("notify", new { type, payload }, ct);
}
