using Microsoft.EntityFrameworkCore;
using VMP.Common.Databases.Base;
using VMP.Common.Databases.Implements;
using VMP.Vehicle.Domain.Entities;
using VMP.Vehicle.Domain.Repositories.Interfaces;
using VMP.Vehicle.Infrastructure.Data;

namespace VMP.Vehicle.Infrastructure.Repositories.Implements
{
    public class VehicleTypeBrandRepository : PostgresRepository<VehicleTypeBrand>, IVehicleTypeBrandRepository
    {
        public VehicleTypeBrandRepository(VehicleDbContext context) : base(context)
        {
        }

        public async Task<List<VehicleBrand>> GetBrandsByTypeIdAsync(Guid typeId)
        {
            return await _context.Set<VehicleTypeBrand>()
                .Where(vtb => vtb.VehicleTypeId == typeId && vtb.DeletedAt == null && vtb.Status == EntityStatus.Active)
                .Include(vtb => vtb.VehicleBrand)
                .Select(vtb => vtb.VehicleBrand)
                .Where(b => b.DeletedAt == null && b.Status == EntityStatus.Active)
                .ToListAsync();
        }

        public async Task<List<VehicleType>> GetTypesByBrandIdAsync(Guid brandId)
        {
            return await _context.Set<VehicleTypeBrand>()
                .Where(vtb => vtb.VehicleBrandId == brandId && vtb.DeletedAt == null && vtb.Status == EntityStatus.Active)
                .Include(vtb => vtb.VehicleType)
                .Select(vtb => vtb.VehicleType)
                .Where(t => t.DeletedAt == null && t.Status == EntityStatus.Active)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid typeId, Guid brandId)
        {
            return await _context.Set<VehicleTypeBrand>()
                .AnyAsync(vtb => vtb.VehicleTypeId == typeId 
                    && vtb.VehicleBrandId == brandId 
                    && vtb.DeletedAt == null);
        }

        public async Task<VehicleTypeBrand?> GetByTypeAndBrandAsync(Guid typeId, Guid brandId)
        {
            return await _context.Set<VehicleTypeBrand>()
                .Include(vtb => vtb.VehicleType)
                .Include(vtb => vtb.VehicleBrand)
                .FirstOrDefaultAsync(vtb => vtb.VehicleTypeId == typeId 
                    && vtb.VehicleBrandId == brandId 
                    && vtb.DeletedAt == null);
        }
    }
}
