using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public sealed class NotificationsHub : Hub
{
    public const string HubPath = "/hubs/notifications";

    // Optional: client calls to join role groups, etc
    public Task JoinRole(string role) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"role:{role}");
}
