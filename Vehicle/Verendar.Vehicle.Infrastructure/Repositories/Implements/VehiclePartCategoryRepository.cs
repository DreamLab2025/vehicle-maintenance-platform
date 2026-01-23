using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Implements;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Repositories.Interfaces;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
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
