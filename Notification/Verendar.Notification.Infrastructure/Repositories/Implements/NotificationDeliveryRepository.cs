using Verendar.Notification.Domain.Repositories.Interfaces;
namespace Verendar.Notification.Infrastructure.Repositories.Implements
{
    public class NotificationDeliveryRepository(NotificationDbContext context) : PostgresRepository<Domain.Entities.NotificationDelivery>(context), INotificationDeliveryRepository
    {
    }
}
