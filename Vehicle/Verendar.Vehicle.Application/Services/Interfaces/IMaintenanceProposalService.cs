namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IMaintenanceProposalService
    {
        Task<ApiResponse<List<MaintenanceProposalResponse>>> GetPendingByVehicleAsync(
            Guid userId, Guid vehicleId, PaginationRequest pagination, CancellationToken ct = default);

        Task<ApiResponse<MaintenanceProposalResponse>> UpdateAsync(
            Guid userId, Guid vehicleId, Guid proposalId, UpdateProposalRequest request, CancellationToken ct = default);

        Task<ApiResponse<ApplyProposalResult>> ApplyAsync(
            Guid userId, Guid vehicleId, Guid proposalId, CancellationToken ct = default);
    }
}
