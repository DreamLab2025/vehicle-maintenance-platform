using Microsoft.EntityFrameworkCore;
using VMP.Common.Databases.Implements;
using VMP.Vehicle.Domain.Entities;
using VMP.Vehicle.Domain.Repositories.Interfaces;
using VMP.Vehicle.Infrastructure.Data;

namespace VMP.Vehicle.Infrastructure.Repositories.Implements
{
    public class UserVehicleRepository : PostgresRepository<UserVehicle>, IUserVehicleRepository
    {
        public UserVehicleRepository(VehicleDbContext context) : base(context)
        {
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
    }
}
