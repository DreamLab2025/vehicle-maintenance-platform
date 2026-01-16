using Verendar.Common.Databases.Implements;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verendar.Notification.Infrastructure.Data;

namespace Verendar.Notification.Infrastructure.Repositories.Implements;

public class NotificationTemplateChannelRepository : PostgresRepository<Domain.Entities.NotificationTemplateChannel>, INotificationTemplateChannelRepository
{
    public NotificationTemplateChannelRepository(NotificationDbContext context) : base(context)
    {
    }
}
