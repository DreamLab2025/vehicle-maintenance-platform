using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;
using Verendar.Common.Databases.Implements;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class UserVehicleRepository : PostgresRepository<UserVehicle>, IUserVehicleRepository
    {
        private readonly VehicleDbContext _context;
        public UserVehicleRepository(VehicleDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<UserVehicle> GetQueryWithFullDetails()
        {
            return _dbSet
                .Include(v => v.VehicleVariant)
                    .ThenInclude(vv => vv.VehicleModel)
                        .ThenInclude(vm => vm.Brand)
                .Include(v => v.VehicleVariant)
                    .ThenInclude(vv => vv.VehicleModel)
                        .ThenInclude(vm => vm.Type)
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
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-(daysRequired - 1));

            var logDates = await _context.OdometerHistories
                .Where(x => x.UserVehicleId == vehicleId
                            && x.RecordedAt >= startDate
                            && x.RecordedAt <= DateTime.UtcNow)
                .Select(x => x.RecordedAt.Date)
                .Distinct()
                .ToListAsync();

            return logDates.Count >= daysRequired;
        }
    }
}
