namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IUserVehicleService
    {
        Task<ApiResponse<List<UserVehicleSummaryDto>>> GetUserVehiclesAsync(Guid userId, PaginationRequest paginationRequest);
        Task<ApiResponse<UserVehicleDetailResponse>> GetUserVehicleByIdAsync(Guid userId, Guid vehicleId);
        Task<ApiResponse<UserVehicleResponse>> CreateUserVehicleAsync(Guid userId, UserVehicleRequest request);
        Task<ApiResponse<UserVehicleResponse>> UpdateUserVehicleAsync(Guid userId, Guid vehicleId, UserVehicleRequest request);
        Task<ApiResponse<string>> DeleteUserVehicleAsync(Guid userId, Guid vehicleId);
        Task<ApiResponse<UserVehicleResponse>> CompleteOnboardingAsync(Guid userId, Guid vehicleId);
        Task<ApiResponse<IsAllowedToCreateVehicleResponse>> IsAllowedToCreateVehicleAsync(Guid userId);
        Task<ApiResponse<VehicleHealthScoreResponse>> GetHealthScoreAsync(Guid userId, Guid vehicleId);
    }
}
