using Verendar.Common.Databases.UnitOfWork;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verendar.Notification.Infrastructure.Data;

namespace Verendar.Notification.Infrastructure.Repositories.Implements;

public class UnitOfWork(NotificationDbContext context) : BaseUnitOfWork<NotificationDbContext>(context), IUnitOfWork
{
    private INotificationRepository? _notifications;
    private INotificationTemplateRepository? _notificationTemplates;
    private INotificationDeliveryRepository? _notificationDeliveries;
    private INotificationPreferenceRepository? _notificationPreferences;
    private INotificationTemplateChannelRepository? _notificationTemplateChannels;

    public INotificationRepository Notifications => _notifications ??= new NotificationRepository(Context);
    public INotificationTemplateRepository NotificationTemplates => _notificationTemplates ??= new NotificationTemplateRepository(Context);
    public INotificationDeliveryRepository NotificationDeliveries => _notificationDeliveries ??= new NotificationDeliveryRepository(Context);
    public INotificationPreferenceRepository NotificationPreferences => _notificationPreferences ??= new NotificationPreferenceRepository(Context);
    public INotificationTemplateChannelRepository NotificationTemplateChannels => _notificationTemplateChannels ??= new NotificationTemplateChannelRepository(Context);
}
