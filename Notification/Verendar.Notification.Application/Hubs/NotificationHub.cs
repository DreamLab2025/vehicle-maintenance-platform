using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Verendar.Notification.Application.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
