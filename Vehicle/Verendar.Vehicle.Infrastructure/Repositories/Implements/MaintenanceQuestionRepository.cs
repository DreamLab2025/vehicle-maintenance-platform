using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class MaintenanceQuestionRepository(VehicleDbContext context) : IMaintenanceQuestionRepository
    {
        private readonly VehicleDbContext _context = context;

        public async Task<IReadOnlyList<MaintenanceQuestion>> GetActiveForPartCategoryAsync(
            Guid partCategoryId,
            CancellationToken cancellationToken = default)
        {
            return await _context.MaintenanceQuestions
                .AsNoTracking()
                .Include(q => q.Group)
                .Include(q => q.Options.OrderBy(o => o.DisplayOrder))
                .Where(q =>
                    q.AppliesToAllPartCategories
                    || q.PartCategoryLinks.Any(l => l.PartCategoryId == partCategoryId))
                .OrderBy(q => q.Group.DisplayOrder)
                .ThenBy(q => q.DisplayOrder)
                .ToListAsync(cancellationToken);
        }
    }
}
