using Verendar.Notification.Domain.Repositories.Interfaces;
namespace Verendar.Notification.Infrastructure.Repositories.Implements
{
    public class NotificationPreferenceRepository(NotificationDbContext context) : PostgresRepository<Domain.Entities.NotificationPreference>(context), INotificationPreferenceRepository
    {
    }
}
