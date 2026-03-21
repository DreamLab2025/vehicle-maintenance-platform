using Verendar.Notification.Domain.Repositories.Interfaces;
namespace Verendar.Notification.Infrastructure.Repositories.Implements
{
    public class NotificationTemplateChannelRepository(NotificationDbContext context) : PostgresRepository<Domain.Entities.NotificationTemplateChannel>(context), INotificationTemplateChannelRepository
    {
    }
}
