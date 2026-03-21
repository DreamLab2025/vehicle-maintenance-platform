namespace Verendar.Notification.Application.Services.Interfaces
{
    public interface IChannelFactory
    {
        INotificationChannel GetChannel(NotificationChannel type);
    }
}
