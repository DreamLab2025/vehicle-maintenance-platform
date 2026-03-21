using Verendar.Notification.Domain.Repositories.Interfaces;
namespace Verendar.Notification.Infrastructure.Repositories.Implements
{
    public class NotificationTemplateRepository(NotificationDbContext context) : PostgresRepository<Domain.Entities.NotificationTemplate>(context), INotificationTemplateRepository
    {
    }
}
