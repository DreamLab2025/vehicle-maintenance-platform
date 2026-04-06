using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IMaintenanceQuestionRepository
    {
        Task<IReadOnlyList<MaintenanceQuestion>> GetActiveForPartCategoryAsync(
            Guid partCategoryId,
            CancellationToken cancellationToken = default);
    }
}
