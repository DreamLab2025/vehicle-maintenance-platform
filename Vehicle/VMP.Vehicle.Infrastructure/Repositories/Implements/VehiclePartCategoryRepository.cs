using Microsoft.EntityFrameworkCore;
using VMP.Common.Databases.Implements;
using VMP.Vehicle.Domain.Entities;
using VMP.Vehicle.Domain.Repositories.Interfaces;
using VMP.Vehicle.Infrastructure.Data;

namespace VMP.Vehicle.Infrastructure.Repositories.Implements
{
    public class VehiclePartCategoryRepository : PostgresRepository<VehiclePartCategory>, IVehiclePartCategoryRepository
    {
        public VehiclePartCategoryRepository(VehicleDbContext context) : base(context)
        {
        }

        public async Task<VehiclePartCategory?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Code == code && c.DeletedAt == null);
        }
    }
}
