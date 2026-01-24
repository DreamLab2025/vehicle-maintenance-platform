using Verendar.Common.Databases.Implements;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verendar.Notification.Infrastructure.Data;

namespace Verendar.Notification.Infrastructure.Repositories.Implements;

public class NotificationTemplateRepository : PostgresRepository<Domain.Entities.NotificationTemplate>, INotificationTemplateRepository
{
    public NotificationTemplateRepository(NotificationDbContext context) : base(context)
    {
    }
}
