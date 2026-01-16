using Microsoft.EntityFrameworkCore;
using Verender.Common.Databases.Implements;
using Verender.Vehicle.Domain.Entities;
using Verender.Vehicle.Domain.Repositories.Interfaces;
using Verender.Vehicle.Infrastructure.Data;

namespace Verender.Vehicle.Infrastructure.Repositories.Implements
{
    public class OdometerHistoryRepository : PostgresRepository<OdometerHistory>, IOdometerHistoryRepository
    {
        public OdometerHistoryRepository(VehicleDbContext context) : base(context)
        {
        }

        public async Task<int> GetCurrentStreakAsync(Guid userVehicleId)
        {
            var logDates = await _dbSet
                .Where(x => x.UserVehicleId == userVehicleId && x.DeletedAt == null)
                .OrderByDescending(x => x.RecordedAt)
                .Select(x => x.RecordedAt.Date)
                .Distinct()
                .Take(100)
                .ToListAsync();

            if (!logDates.Any())
            {
                return 0;
            }

            var today = DateTime.UtcNow.Date;
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
    }
}
