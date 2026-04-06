using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Domain.Repositories.Interfaces
{
    public interface IMaintenanceProposalRepository : IGenericRepository<MaintenanceProposal>
    {
        Task<MaintenanceProposal?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
        Task<MaintenanceProposal?> GetByIdTrackedWithItemsAsync(Guid id, CancellationToken ct = default);
        Task<(List<MaintenanceProposal> Items, int TotalCount)> GetPagedPendingByVehicleIdAsync(
            Guid vehicleId, int pageNumber, int pageSize, CancellationToken ct = default);
        Task<bool> ExistsByBookingIdAsync(Guid bookingId, CancellationToken ct = default);
    }
}
