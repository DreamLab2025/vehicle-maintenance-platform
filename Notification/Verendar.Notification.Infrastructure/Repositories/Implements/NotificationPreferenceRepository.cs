using Verendar.Common.Databases.Implements;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verendar.Notification.Infrastructure.Data;

namespace Verendar.Notification.Infrastructure.Repositories.Implements
{
    public class NotificationPreferenceRepository(NotificationDbContext context) : PostgresRepository<Domain.Entities.NotificationPreference>(context), INotificationPreferenceRepository
    {
    }
}
