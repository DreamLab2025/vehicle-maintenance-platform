using Verendar.Vehicle.Domain.Enums;
using Verendar.Vehicle.Domain.Repositories.Interfaces;

namespace Verendar.Vehicle.Infrastructure.Repositories.Implements
{
    public class MaintenanceProposalRepository(VehicleDbContext context)
        : PostgresRepository<MaintenanceProposal>(context), IMaintenanceProposalRepository
    {
        public async Task<MaintenanceProposal?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
        {
            return await context.MaintenanceProposals
                .Include(p => p.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, ct);
        }

        public async Task<MaintenanceProposal?> GetByIdTrackedWithItemsAsync(Guid id, CancellationToken ct = default)
        {
            return await context.MaintenanceProposals
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null, ct);
        }

        public async Task<(List<MaintenanceProposal> Items, int TotalCount)> GetPagedPendingByVehicleIdAsync(
            Guid vehicleId, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var query = context.MaintenanceProposals
                .Include(p => p.Items)
                .AsNoTracking()
                .Where(p => p.UserVehicleId == vehicleId
                         && p.Status == ProposalStatus.Pending
                         && p.DeletedAt == null)
                .OrderByDescending(p => p.CreatedAt);

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<bool> ExistsByBookingIdAsync(Guid bookingId, CancellationToken ct = default)
        {
            return await context.MaintenanceProposals
                .AnyAsync(p => p.BookingId == bookingId && p.DeletedAt == null, ct);
        }
    }
}
