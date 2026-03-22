namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IOdometerHistoryService
    {
        Task<ApiResponse<UpdateOdometerResponse>> UpdateOdometerAsync(Guid userId, Guid vehicleId, UpdateOdometerRequest request);
        Task<ApiResponse<List<OdometerHistoryItemDto>>> GetOdometerHistoryPagedAsync(Guid userId, Guid userVehicleId, OdometerHistoryQueryRequest query);
        Task<ApiResponse<StreakResponse>> GetVehicleStreakAsync(Guid userId, Guid userVehicleId);
    }
}
