using Microsoft.EntityFrameworkCore;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class BrandRepository(VehicleDbContext context) : PostgresRepository<Brand>(context), IBrandRepository
    {
        public IQueryable<Brand> AsQueryableWithVehicleType() =>
            _dbSet.Include(b => b.VehicleType);

        public async Task<Brand?> GetByIdWithTypesAsync(Guid id)
        {
            return await _context.Set<Brand>()
                .Include(b => b.VehicleType)
                .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);
        }
    }
}
