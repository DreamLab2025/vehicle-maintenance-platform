using Verendar.Notification.Domain.Repositories.Interfaces;
namespace Verendar.Notification.Infrastructure.Repositories.Implements
{
    public class NotificationRepository(NotificationDbContext context) : PostgresRepository<Domain.Entities.Notification>(context), INotificationRepository
    {
        public async Task<Domain.Entities.Notification?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken);
        }

        public async Task<(List<Domain.Entities.Notification> Items, int TotalCount)> GetByUserIdWithInAppChannelPagedAsync(
            Guid userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = context.Notifications
                .Where(n => n.UserId == userId && n.Deliveries.Any(d => d.Channel == NotificationChannel.InApp))
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && n.Deliveries.Any(d => d.Channel == NotificationChannel.InApp))
                .CountAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Domain.Entities.Notification>> GetUnreadByUserIdWithInAppAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && n.Deliveries.Any(d => d.Channel == NotificationChannel.InApp))
                .ToListAsync(cancellationToken);
        }
    }
}
