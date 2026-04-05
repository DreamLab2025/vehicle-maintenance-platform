namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class GarageServiceRepository(GarageDbContext context)
    : PostgresRepository<GarageService>(context), IGarageServiceRepository
{
    private readonly GarageDbContext _db = context;

    public async Task<GarageService?> GetByIdWithCategoryAsync(Guid id, CancellationToken ct = default) =>
        await _db.Set<GarageService>()
            .AsNoTracking()
            .Include(s => s.ServiceCategory)
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, ct);

    public async Task<GarageService?> GetByIdWithCategoryForUpdateAsync(Guid id, CancellationToken ct = default) =>
        await _db.Set<GarageService>()
            .Include(s => s.ServiceCategory)
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, ct);

    public async Task<(List<GarageService> Items, int TotalCount)> GetPagedByBranchIdAsync(
        Guid branchId,
        bool activeOnly,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.Set<GarageService>()
            .AsNoTracking()
            .Where(s => s.GarageBranchId == branchId && s.DeletedAt == null);

        if (activeOnly)
            query = query.Where(s => s.Status == ProductStatus.Active);

        query = query.OrderBy(s => s.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
