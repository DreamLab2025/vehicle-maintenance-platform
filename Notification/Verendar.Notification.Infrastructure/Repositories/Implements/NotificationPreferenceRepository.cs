using Verendar.Common.Databases.Implements;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verendar.Notification.Infrastructure.Data;

namespace Verendar.Notification.Infrastructure.Repositories.Implements;

public class NotificationPreferenceRepository : PostgresRepository<Domain.Entities.NotificationPreference>, INotificationPreferenceRepository
{
    public NotificationPreferenceRepository(NotificationDbContext context) : base(context)
    {
    }
}
