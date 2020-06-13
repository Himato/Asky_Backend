using System;
using System.Threading.Tasks;
using Asky.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Asky.Hubs
{
    [Authorize]
    public class NotificationsHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Context.User.Identity.GetUserId());
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.User.Identity.GetUserId());
        }
    }

    public static class NotificationsHubExtensions
    {
        public static void Notify(this IHubContext<NotificationsHub> hub, string receiverId, NotificationDto notification)
        {
            hub.Clients.Group(receiverId).SendAsync("Notify", notification);
        }
    }
}
