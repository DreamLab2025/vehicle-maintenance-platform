namespace Verendar.Location.Infrastructure.Repositories.Implements;

using Verendar.Location.Domain.Entities;
using Verendar.Location.Domain.Repositories.Interfaces;

public class AdministrativeRegionRepository(LocationDbContext context) : IAdministrativeRegionRepository
{
    private readonly LocationDbContext _context = context;

    public async Task<List<AdministrativeRegion>> GetAllAsync()
    {
        return await _context.AdministrativeRegions.ToListAsync();
    }

    public async Task<AdministrativeRegion?> GetByIdAsync(int id)
    {
        return await _context.AdministrativeRegions.FirstOrDefaultAsync(r => r.Id == id);
    }
}
