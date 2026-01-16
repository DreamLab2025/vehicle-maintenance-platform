using Verendar.Common.Databases.Implements;
using Verendar.Notification.Domain.Repositories.Interfaces;
using Verendar.Notification.Infrastructure.Data;

namespace Verendar.Notification.Infrastructure.Repositories.Implements;

public class NotificationDeliveryRepository : PostgresRepository<Domain.Entities.NotificationDelivery>, INotificationDeliveryRepository
{
    public NotificationDeliveryRepository(NotificationDbContext context) : base(context)
    {
    }
}
