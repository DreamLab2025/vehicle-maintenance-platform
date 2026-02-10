using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Verendar.Notification.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
