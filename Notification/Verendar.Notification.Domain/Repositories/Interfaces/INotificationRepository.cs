using Verendar.Common.Databases.Interfaces;

namespace Verendar.Notification.Domain.Repositories.Interfaces;

public interface INotificationRepository : IGenericRepository<Entities.Notification>
{
    Task<Entities.Notification?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<(List<Entities.Notification> Items, int TotalCount)> GetByUserIdWithInAppChannelPagedAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
