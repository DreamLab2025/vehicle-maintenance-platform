namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IOdometerHistoryService
    {
        Task<ApiResponse<UpdateOdometerResponse>> UpdateOdometerAsync(Guid userId, Guid vehicleId, UpdateOdometerRequest request);
        Task<ApiResponse<UpdateOdometerResponse>> FromScanOdometerAsync(Guid userId, Guid vehicleId, FromScanOdometerRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<OdometerHistoryItemDto>>> GetOdometerHistoryPagedAsync(Guid userId, Guid userVehicleId, OdometerHistoryQueryRequest query);
        Task<ApiResponse<StreakResponse>> GetVehicleStreakAsync(Guid userId, Guid userVehicleId);
    }
}
