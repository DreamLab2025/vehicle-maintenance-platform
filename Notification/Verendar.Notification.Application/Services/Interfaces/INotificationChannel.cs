using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Domain.Enums;

namespace Verendar.Notification.Application.Services.Interfaces;

public interface INotificationChannel
{
    NotificationChannel ChannelType { get; }
    Task<ChannelDeliveryResult> SendAsync(NotificationDeliveryContext context);
}
