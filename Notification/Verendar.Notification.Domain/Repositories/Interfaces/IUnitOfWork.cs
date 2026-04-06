using Verendar.Common.Databases.UnitOfWork;

namespace Verendar.Notification.Domain.Repositories.Interfaces
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        INotificationRepository Notifications { get; }
        INotificationDeliveryRepository NotificationDeliveries { get; }
        INotificationPreferenceRepository NotificationPreferences { get; }
    }
}
