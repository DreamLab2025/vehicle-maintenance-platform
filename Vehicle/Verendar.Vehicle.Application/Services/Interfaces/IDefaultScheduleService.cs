namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IDefaultScheduleService
    {
        Task<ApiResponse<List<PartCategoryResponse>>> GetPartCategoriesByVehicleModelAsync(
            Guid vehicleModelId,
            CancellationToken cancellationToken = default);
    }
}
