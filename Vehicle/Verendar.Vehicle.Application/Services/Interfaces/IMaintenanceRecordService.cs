namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IMaintenanceRecordService
    {
        Task<ApiResponse<CreateRecordResponse>> CreateMaintenanceRecordAsync(Guid userId, Guid vehicleId, CreateRecordRequest request);
        Task<ApiResponse<IReadOnlyList<RecordSummaryDto>>> GetMaintenanceHistoryAsync(Guid userId, Guid userVehicleId);
        Task<ApiResponse<RecordDetailDto>> GetMaintenanceRecordDetailAsync(Guid userId, Guid maintenanceRecordId);
    }
}
