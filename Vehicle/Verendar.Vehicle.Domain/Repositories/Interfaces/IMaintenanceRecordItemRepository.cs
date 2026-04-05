using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IMaintenanceRecordItemRepository : IGenericRepository<MaintenanceRecordItem>
    {
        Task<IEnumerable<MaintenanceRecordItem>> GetByMaintenanceRecordIdAsync(Guid maintenanceRecordId, CancellationToken cancellationToken = default);
        Task<List<(Guid PartCategoryId, string Name, int RecordCount, decimal TotalCost)>> GetTopPartCategoriesAsync(DateOnly? from, DateOnly? to, int limit, CancellationToken ct = default);
    }
}
