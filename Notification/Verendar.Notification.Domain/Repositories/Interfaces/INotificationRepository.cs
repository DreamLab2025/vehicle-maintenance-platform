namespace Verendar.Notification.Domain.Repositories.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Entities.Notification>
    {
        Task<Entities.Notification?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

        Task<(List<Entities.Notification> Items, int TotalCount)> GetByUserIdPagedAsync(
            Guid userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Entities.Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
