namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IMaintenanceRecordService
    {
        Task<ApiResponse<CreateMaintenanceRecordResponse>> CreateMaintenanceRecordAsync(Guid userId, Guid vehicleId, CreateMaintenanceRecordRequest request);
        Task<ApiResponse<IReadOnlyList<MaintenanceRecordSummaryDto>>> GetMaintenanceHistoryAsync(Guid userId, Guid userVehicleId);
        Task<ApiResponse<MaintenanceRecordDetailDto>> GetMaintenanceRecordDetailAsync(Guid userId, Guid maintenanceRecordId);
    }
}
