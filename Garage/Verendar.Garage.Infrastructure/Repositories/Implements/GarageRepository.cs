using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using GarageEntity = global::Verendar.Garage.Domain.Entities.Garage;

namespace Verendar.Garage.Infrastructure.Repositories.Implements;

public class GarageRepository(GarageDbContext context)
    : PostgresRepository<GarageEntity>(context), IGarageRepository
{
    public async Task<GarageEntity?> GetWithBranchesAsync(
        Expression<Func<GarageEntity, bool>> filter,
        CancellationToken ct = default)
    {
        return await context.Set<GarageEntity>()
            .Include(g => g.Branches)
            .Where(g => g.DeletedAt == null)
            .FirstOrDefaultAsync(filter, ct);
    }
}
