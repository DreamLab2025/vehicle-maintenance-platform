using Verendar.Common.Databases.Implements;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verendar.Notification.Infrastructure.Data;

namespace Verendar.Notification.Infrastructure.Repositories.Implements;

public class NotificationRepository : PostgresRepository<Domain.Entities.Notification>, INotificationRepository
{
    public NotificationRepository(NotificationDbContext context) : base(context)
    {
    }
}
