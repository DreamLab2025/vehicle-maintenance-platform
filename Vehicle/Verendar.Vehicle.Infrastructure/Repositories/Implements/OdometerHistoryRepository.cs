using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Implements;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;

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
    }
}
