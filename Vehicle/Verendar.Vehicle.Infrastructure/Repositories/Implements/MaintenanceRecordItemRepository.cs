using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class MaintenanceRecordItemRepository(VehicleDbContext context) : PostgresRepository<MaintenanceRecordItem>(context), IMaintenanceRecordItemRepository
    {
        public async Task<IEnumerable<MaintenanceRecordItem>> GetByMaintenanceRecordIdAsync(Guid maintenanceRecordId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PartCategory)
                .Where(x => x.MaintenanceRecordId == maintenanceRecordId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<(Guid PartCategoryId, string Name, int RecordCount, decimal TotalCost)>> GetTopPartCategoriesAsync(
            DateOnly? from,
            DateOnly? to,
            int limit,
            CancellationToken ct = default)
        {
            var query = _dbSet
                .Include(x => x.MaintenanceRecord)
                .Include(x => x.PartCategory)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(x => x.MaintenanceRecord.ServiceDate >= from.Value);
            if (to.HasValue)
                query = query.Where(x => x.MaintenanceRecord.ServiceDate <= to.Value);

            var result = await query
                .GroupBy(x => new { x.PartCategoryId, x.PartCategory.Name })
                .Select(g => new
                {
                    g.Key.PartCategoryId,
                    g.Key.Name,
                    RecordCount = g.Count(),
                    TotalCost = g.Sum(x => x.Price)
                })
                .OrderByDescending(x => x.RecordCount)
                .Take(limit)
                .ToListAsync(ct);

            return result
                .Select(x => (x.PartCategoryId, x.Name, x.RecordCount, x.TotalCost))
                .ToList();
        }
    }
}
