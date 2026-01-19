using Microsoft.EntityFrameworkCore;
using VMP.Common.Databases.Implements;
using VMP.Vehicle.Domain.Entities;
using VMP.Vehicle.Domain.Repositories.Interfaces;
using VMP.Vehicle.Infrastructure.Data;

namespace VMP.Vehicle.Infrastructure.Repositories.Implements
{
    public class OilRepository : PostgresRepository<Oil>, IOilRepository
    {
        public OilRepository(VehicleDbContext context) : base(context)
        {
        }

        public async Task<Oil?> GetByVehiclePartIdAsync(Guid vehiclePartId)
        {
            return await _dbSet
                .Include(o => o.VehiclePart)
                    .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(o => o.VehiclePartId == vehiclePartId && o.DeletedAt == null);
        }

        public async Task<List<Oil>> GetByVehicleUsageAsync(OilVehicleUsage vehicleUsage)
        {
            return await _dbSet
                .Where(o => o.VehicleUsage == vehicleUsage || o.VehicleUsage == OilVehicleUsage.Both)
                .Include(o => o.VehiclePart)
                    .ThenInclude(p => p.Category)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Oil>> GetByViscosityGradeAsync(string viscosityGrade)
        {
            return await _dbSet
                .Where(o => o.ViscosityGrade == viscosityGrade && o.DeletedAt == null)
                .Include(o => o.VehiclePart)
                    .ThenInclude(p => p.Category)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
