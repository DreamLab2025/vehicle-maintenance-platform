namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class ServiceCategoryRepository(GarageDbContext context)
    : PostgresRepository<ServiceCategory>(context), IServiceCategoryRepository
{
    private readonly GarageDbContext _db = context;

    public async Task<ServiceCategory?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await _db.Set<ServiceCategory>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug && c.DeletedAt == null, ct);

    public async Task<List<ServiceCategory>> GetAllOrderedAsync(CancellationToken ct = default) =>
        await _db.Set<ServiceCategory>()
            .AsNoTracking()
            .Where(c => c.DeletedAt == null)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
}
