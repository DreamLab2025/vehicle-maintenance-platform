namespace Verendar.Location.Infrastructure.Repositories.Implements;

using Verendar.Location.Domain.Entities;
using Verendar.Location.Domain.Repositories.Interfaces;

public class AdministrativeUnitRepository(LocationDbContext context) : IAdministrativeUnitRepository
{
    private readonly LocationDbContext _context = context;

    public async Task<List<AdministrativeUnit>> GetAllAsync()
    {
        return await _context.AdministrativeUnits.ToListAsync();
    }

    public async Task<AdministrativeUnit?> GetByIdAsync(int id)
    {
        return await _context.AdministrativeUnits.FirstOrDefaultAsync(u => u.Id == id);
    }
}
