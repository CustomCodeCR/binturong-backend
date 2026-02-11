using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Notifications;

public sealed class NotificationsHub : Hub
{
    public const string HubPath = "/hubs/notifications";

    public override async Task OnConnectedAsync()
    {
        // Si querés auto-join a grupos por claims, lo hacemos después.
        await base.OnConnectedAsync();
    }
}
