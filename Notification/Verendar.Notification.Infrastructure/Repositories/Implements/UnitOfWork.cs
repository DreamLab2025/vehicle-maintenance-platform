using Verendar.Common.Databases.UnitOfWork;
using Verendar.Notification.Domain.Repositories.Interfaces;

namespace Verendar.Notification.Infrastructure.Repositories.Implements;

public class UnitOfWork(NotificationDbContext context) : BaseUnitOfWork<NotificationDbContext>(context), IUnitOfWork
{
    private INotificationRepository? _notifications;
    private INotificationDeliveryRepository? _notificationDeliveries;
    private INotificationPreferenceRepository? _notificationPreferences;

    public INotificationRepository Notifications => _notifications ??= new NotificationRepository(Context);
    public INotificationDeliveryRepository NotificationDeliveries => _notificationDeliveries ??= new NotificationDeliveryRepository(Context);
    public INotificationPreferenceRepository NotificationPreferences => _notificationPreferences ??= new NotificationPreferenceRepository(Context);
}
