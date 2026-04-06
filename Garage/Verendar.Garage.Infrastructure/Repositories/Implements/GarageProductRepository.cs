namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class GarageProductRepository(GarageDbContext context)
    : PostgresRepository<GarageProduct>(context), IGarageProductRepository
{
    private readonly GarageDbContext _db = context;

    public async Task<GarageProduct?> GetByIdWithInstallationAsync(Guid id, CancellationToken ct = default) =>
        await _db.Set<GarageProduct>()
            .AsNoTracking()
            .Include(p => p.InstallationService)
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, ct);

    public async Task<(List<GarageProduct> Items, int TotalCount)> GetPagedByBranchIdAsync(
        Guid branchId,
        bool activeOnly,
        int pageNumber,
        int pageSize,
        string? name = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        Guid? categoryId = null,
        CancellationToken ct = default)
    {
        var query = _db.Set<GarageProduct>()
            .AsNoTracking()
            .Where(p => p.GarageBranchId == branchId && p.DeletedAt == null);

        if (activeOnly)
            query = query.Where(p => p.Status == ProductStatus.Active);
        if (name is not null)
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{name}%"));
        if (minPrice.HasValue)
            query = query.Where(p => p.MaterialPrice.Amount >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(p => p.MaterialPrice.Amount <= maxPrice.Value);
        if (categoryId.HasValue)
            query = query.Where(p => p.PartCategoryId == categoryId.Value);

        query = query.OrderBy(p => p.Name);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Include(p => p.InstallationService)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
