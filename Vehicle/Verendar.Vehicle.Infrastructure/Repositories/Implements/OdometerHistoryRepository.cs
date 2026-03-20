using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class OdometerHistoryRepository(VehicleDbContext context) : PostgresRepository<OdometerHistory>(context), IOdometerHistoryRepository
    {
        public async Task<int> GetCurrentStreakAsync(Guid userVehicleId)
        {
            var logDates = await _dbSet
                .Where(x => x.UserVehicleId == userVehicleId && x.DeletedAt == null)
                .OrderByDescending(x => x.RecordedDate)
                .Select(x => x.RecordedDate)
                .Distinct()
                .Take(100)
                .ToListAsync();

            if (!logDates.Any())
            {
                return 0;
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var yesterday = today.AddDays(-1);
            var lastLogDate = logDates.First();

            if (lastLogDate < yesterday)
            {
                return 0;
            }

            int streak = 0;
            var expectedDate = lastLogDate;

            foreach (var date in logDates)
            {
                if (date == expectedDate)
                {
                    streak++;
                    expectedDate = expectedDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        public async Task<(IEnumerable<OdometerHistory> Items, int TotalCount)> GetPagedByUserVehicleAsync(
            Guid userVehicleId,
            int pageNumber,
            int pageSize,
            DateOnly? fromDate,
            DateOnly? toDate,
            bool isDescending = true,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(x => x.UserVehicleId == userVehicleId && x.DeletedAt == null);

            if (fromDate.HasValue)
                query = query.Where(x => x.RecordedDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(x => x.RecordedDate <= toDate.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var ordered = isDescending
                ? query.OrderByDescending(x => x.RecordedDate).ThenByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.RecordedDate).ThenBy(x => x.CreatedAt);

            var items = await ordered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
