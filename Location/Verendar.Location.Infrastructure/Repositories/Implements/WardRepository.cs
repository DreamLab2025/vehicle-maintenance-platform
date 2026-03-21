namespace Verendar.Location.Infrastructure.Repositories.Implements;

using Verendar.Location.Domain.Entities;
using Verendar.Location.Domain.Repositories.Interfaces;

public class WardRepository(LocationDbContext context) : IWardRepository
{
    private readonly LocationDbContext _context = context;

    public async Task<Ward?> GetByCodeAsync(string code)
    {
        return await _context.Wards
            .Include(w => w.Province)
            .Include(w => w.AdministrativeUnit)
            .FirstOrDefaultAsync(w => w.Code == code);
    }

    public async Task<List<Ward>> GetByProvinceCodeAsync(string provinceCode)
    {
        return await _context.Wards
            .Include(w => w.Province)
            .Include(w => w.AdministrativeUnit)
            .Where(w => w.ProvinceCode == provinceCode)
            .ToListAsync();
    }
}
