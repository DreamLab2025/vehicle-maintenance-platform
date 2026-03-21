namespace Verendar.Notification.Application.Services.Interfaces
{
    public interface INotificationChannel
    {
        NotificationChannel ChannelType { get; }
        Task<ChannelDeliveryResult> SendAsync(NotificationDeliveryContext context);
    }
}
