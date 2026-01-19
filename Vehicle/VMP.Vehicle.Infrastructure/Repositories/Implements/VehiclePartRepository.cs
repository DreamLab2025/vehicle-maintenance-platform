using Microsoft.EntityFrameworkCore;
using VMP.Common.Databases.Implements;
using VMP.Vehicle.Domain.Entities;
using VMP.Vehicle.Domain.Repositories.Interfaces;
using VMP.Vehicle.Infrastructure.Data;

namespace VMP.Vehicle.Infrastructure.Repositories.Implements
{
    public class VehiclePartRepository : PostgresRepository<VehiclePart>, IVehiclePartRepository
    {
        public VehiclePartRepository(VehicleDbContext context) : base(context)
        {
        }

        public async Task<List<VehiclePart>> GetByCategoryIdAsync(Guid categoryId)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId && p.DeletedAt == null)
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<VehiclePart>> GetByCategoryCodeAsync(string categoryCode)
        {
            return await _dbSet
                .Where(p => p.Category.Code == categoryCode && p.DeletedAt == null)
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
