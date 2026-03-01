using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;
using Verendar.Common.Databases.Implements;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class UserVehicleRepository(VehicleDbContext context) : PostgresRepository<UserVehicle>(context), IUserVehicleRepository
    {
        private new readonly VehicleDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public IQueryable<UserVehicle> GetQueryWithFullDetails()
        {
            return _dbSet
                .Include(v => v.Variant)
                    .ThenInclude(vv => vv.VehicleModel)
                        .ThenInclude(vm => vm.Brand)
                            .ThenInclude(b => b.VehicleType)
                .Include(v => v.PartTrackings)
                    .ThenInclude(pt => pt.PartCategory)
                .Include(v => v.PartTrackings)
                    .ThenInclude(pt => pt.CurrentPartProduct)
                .Include(v => v.PartTrackings)
                    .ThenInclude(pt => pt.Reminders)
                .Where(v => v.DeletedAt == null);
        }

        public IQueryable<UserVehicle> GetQueryWithoutPartTrackings()
        {
            return _dbSet
                .Include(v => v.Variant)
                    .ThenInclude(vv => vv.VehicleModel)
                        .ThenInclude(vm => vm.Brand)
                            .ThenInclude(b => b.VehicleType)
                .Where(v => v.DeletedAt == null);
        }

        public async Task<UserVehicle?> GetByIdWithFullDetailsAsync(Guid id)
        {
            return await GetQueryWithFullDetails()
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<UserVehicle?> GetByIdAndUserIdWithFullDetailsAsync(Guid id, Guid userId)
        {
            return await GetQueryWithFullDetails()
                .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);
        }

        public async Task<UserVehicle?> GetByIdAndUserIdWithoutPartTrackingsAsync(Guid id, Guid userId)
        {
            return await GetQueryWithoutPartTrackings()
                .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);
        }

        public async Task<(bool IsAllowed, string Message)> CheckCanCreateVehicleAsync(Guid userId, bool isPremiumUser = false)
        {
            var existingVehicles = await _dbSet
                .Where(v => v.UserId == userId && v.Status == EntityStatus.Active && v.DeletedAt == null)
                .ToListAsync();

            var currentCount = existingVehicles.Count();
            if (currentCount == 0)
            {
                return (true, "Success.");
            }
            if (currentCount == 1)
            {
                var currentVehicleId = existingVehicles.First().Id;

                bool passStreakChallenge = await HasContinuousOdometerUpdatesAsync(currentVehicleId, daysRequired: 7);

                if (passStreakChallenge)
                {
                    return (true, "Success.");
                }

                return (false, "Bạn cần cập nhật Odometer liên tục trong 7 ngày gần nhất cho chiếc xe hiện tại để được thêm xe thứ 2.");
            }
            if (currentCount == 2)
            {
                if (isPremiumUser)
                {
                    return (true, "Success.");
                }

                return (false, "Bạn cần nâng cấp lên tài khoản Premium để sở hữu chiếc xe thứ 3.");
            }
            return (false, "Bạn đã đạt giới hạn số lượng xe tối đa cho phép.");
        }

        private async Task<bool> HasContinuousOdometerUpdatesAsync(Guid vehicleId, int daysRequired)
        {
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var startDate = endDate.AddDays(-(daysRequired - 1));

            var logDates = await _context.OdometerHistories
                .Where(x => x.UserVehicleId == vehicleId
                            && x.RecordedDate >= startDate
                            && x.RecordedDate <= endDate)
                .Select(x => x.RecordedDate)
                .Distinct()
                .ToListAsync();

            return logDates.Count >= daysRequired;
        }

        public async Task<IReadOnlyList<Guid>> GetDistinctUserIdsWithStaleOdometerAsync(int olderThanDays, CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-olderThanDays);

            var userIds = await _dbSet
                .Where(v => v.DeletedAt == null
                            && v.Status == EntityStatus.Active
                            && (v.LastOdometerUpdate == null || v.LastOdometerUpdate < cutoffDate))
                .Select(v => v.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);

            return userIds;
        }

        public async Task<IReadOnlyList<UserVehicle>> GetStaleOdometerVehiclesByUserAsync(Guid userId, int olderThanDays, CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-olderThanDays);

            return await _dbSet
                .Include(v => v.Variant)
                    .ThenInclude(vv => vv.VehicleModel)
                .Where(v => v.DeletedAt == null
                            && v.Status == EntityStatus.Active
                            && v.UserId == userId
                            && (v.LastOdometerUpdate == null || v.LastOdometerUpdate < cutoffDate))
                .ToListAsync(cancellationToken);
        }
    }
}
