namespace Verendar.Location.Infrastructure.Repositories.Implements;

using Verendar.Location.Domain.Entities;
using Verendar.Location.Domain.Repositories.Interfaces;

public class ProvinceRepository(LocationDbContext context) : IProvinceRepository
{
    private readonly LocationDbContext _context = context;

    public async Task<Province?> GetByCodeAsync(string code)
    {
        return await _context.Provinces
            .Include(p => p.AdministrativeRegion)
            .Include(p => p.AdministrativeUnit)
            .FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<List<Province>> GetAllAsync()
    {
        return await _context.Provinces
            .Include(p => p.AdministrativeRegion)
            .Include(p => p.AdministrativeUnit)
            .ToListAsync();
    }

    public async Task<List<Province>> GetAllByRegionIdAsync(int regionId)
    {
        return await _context.Provinces
            .Include(p => p.AdministrativeRegion)
            .Include(p => p.AdministrativeUnit)
            .Where(p => p.AdministrativeRegionId == regionId)
            .ToListAsync();
    }

    public async Task UpdateBoundaryUrlAsync(string code, string url)
    {
        await _context.Provinces
            .Where(p => p.Code == code)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.BoundaryUrl, url));
    }
}
