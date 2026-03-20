namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IDefaultMaintenanceScheduleService
    {
        Task<ApiResponse<List<PartCategoryResponse>>> GetPartCategoriesByVehicleModelAsync(
            Guid vehicleModelId,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<DefaultMaintenanceScheduleResponse>> GetByVehicleModelAndPartCategoryAsync(
            Guid vehicleModelId,
            string partCategoryCode,
            CancellationToken cancellationToken = default);
    }
}
