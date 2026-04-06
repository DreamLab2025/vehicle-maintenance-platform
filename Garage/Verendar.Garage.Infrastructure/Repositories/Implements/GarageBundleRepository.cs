namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class GarageBundleRepository(GarageDbContext context)
    : PostgresRepository<GarageBundle>(context), IGarageBundleRepository
{
    private readonly GarageDbContext _db = context;

    public async Task<GarageBundle?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default) =>
        await _db.Set<GarageBundle>()
            .AsSplitQuery()
            .AsNoTracking()
            .Include(b => b.Items.Where(i => i.DeletedAt == null).OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.InstallationService)
            .Include(b => b.Items.Where(i => i.DeletedAt == null).OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Service)
            .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null, ct);

    public async Task<GarageBundle?> GetByIdWithItemsForUpdateAsync(Guid id, CancellationToken ct = default) =>
        await _db.Set<GarageBundle>()
            .AsSplitQuery()
            .Include(b => b.Items.Where(i => i.DeletedAt == null).OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.InstallationService)
            .Include(b => b.Items.Where(i => i.DeletedAt == null).OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Service)
            .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null, ct);

    public async Task<(List<GarageBundle> Items, int TotalCount)> GetPagedByBranchIdAsync(
        Guid branchId,
        bool activeOnly,
        int pageNumber,
        int pageSize,
        string? name = null,
        CancellationToken ct = default)
    {
        var query = _db.Set<GarageBundle>()
            .AsNoTracking()
            .Where(b => b.GarageBranchId == branchId && b.DeletedAt == null);

        if (activeOnly)
            query = query.Where(b => b.Status == ProductStatus.Active);
        if (name is not null)
            query = query.Where(b => EF.Functions.ILike(b.Name, $"%{name}%"));

        query = query.OrderBy(b => b.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .AsSplitQuery()
            .Include(b => b.Items.Where(i => i.DeletedAt == null).OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.InstallationService)
            .Include(b => b.Items.Where(i => i.DeletedAt == null).OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.Service)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
