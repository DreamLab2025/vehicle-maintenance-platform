using Verendar.Common.Stats;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class MaintenanceRecordRepository(VehicleDbContext context) : PostgresRepository<MaintenanceRecord>(context), IMaintenanceRecordRepository
    {
        public async Task<IEnumerable<MaintenanceRecord>> GetByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.UserVehicleId == userVehicleId)
                .OrderByDescending(x => x.ServiceDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetByUserVehicleIdWithItemsAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.UserVehicleId == userVehicleId)
                .OrderByDescending(x => x.ServiceDate)
                .Include(x => x.Items)
                    .ThenInclude(i => i.PartCategory)
                .ToListAsync(cancellationToken);
        }

        public async Task<MaintenanceRecord?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.Items)
                    .ThenInclude(x => x.PartCategory)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<(int TotalCount, DateOnly? LastServiceDate)> GetActivitySummaryByUserVehicleIdAsync(Guid userVehicleId, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(r => r.UserVehicleId == userVehicleId);
            var count = await query.CountAsync(cancellationToken);
            if (count == 0)
                return (0, null);
            var last = await query.MaxAsync(r => r.ServiceDate, cancellationToken);
            return (count, last);
        }

        public async Task<(int Total, decimal TotalCost)> GetStatsSummaryAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default)
        {
            var query = _dbSet.AsQueryable();
            if (from.HasValue)
                query = query.Where(x => x.ServiceDate >= from.Value);
            if (to.HasValue)
                query = query.Where(x => x.ServiceDate <= to.Value);

            var total = await query.CountAsync(ct);
            var totalCost = total > 0 ? await query.SumAsync(x => x.TotalCost, ct) : 0m;
            return (total, totalCost);
        }

        public async Task<List<ChartPoint>> GetActivityChartAsync(DateTime from, DateTime to, string groupBy, CancellationToken ct = default)
        {
            var fromDate = DateOnly.FromDateTime(from);
            var toDate = DateOnly.FromDateTime(to);

            var query = _dbSet.Where(x => x.ServiceDate >= fromDate && x.ServiceDate <= toDate);

            if (groupBy == "day")
            {
                var raw = await query
                    .GroupBy(x => x.ServiceDate)
                    .Select(g => new { Period = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Period)
                    .ToListAsync(ct);

                return raw
                    .Select(p => new ChartPoint(p.Period.ToString("yyyy-MM-dd"), p.Count))
                    .ToList();
            }
            else
            {
                var raw = await query
                    .GroupBy(x => new { x.ServiceDate.Year, x.ServiceDate.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync(ct);

                return raw
                    .Select(p => new ChartPoint($"{p.Year:D4}-{p.Month:D2}", p.Count))
                    .ToList();
            }
        }
    }
}
