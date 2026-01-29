using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IUserVehicleService
    {
        Task<ApiResponse<List<UserVehicleResponse>>> GetUserVehiclesAsync(Guid userId, PaginationRequest paginationRequest);
        Task<ApiResponse<UserVehicleDetailResponse>> GetUserVehicleByIdAsync(Guid userId, Guid vehicleId);
        Task<ApiResponse<UserVehicleResponse>> CreateUserVehicleAsync(Guid userId, UserVehicleRequest request);
        Task<ApiResponse<UserVehicleResponse>> UpdateUserVehicleAsync(Guid userId, Guid vehicleId, UserVehicleRequest request);
        Task<ApiResponse<UserVehicleResponse>> UpdateOdometerAsync(Guid userId, Guid vehicleId, UpdateOdometerRequest request);
        Task<ApiResponse<string>> DeleteUserVehicleAsync(Guid userId, Guid vehicleId);
        Task<ApiResponse<VehicleStreakResponse>> GetVehicleStreakAsync(Guid userVehicleId);
        Task<ApiResponse<VehiclePartTrackingSummary>> ApplyTrackingConfigAsync(Guid userId, Guid vehicleId, ApplyTrackingConfigRequest request);
        Task<ApiResponse<List<UserVehiclePartSummary>>> GetPartsByUserVehicleAsync(Guid userId, Guid userVehicleId);
        Task<ApiResponse<UserVehicleResponse>> CompleteOnboardingAsync(Guid userId, Guid vehicleId);
        Task<ApiResponse<IsAllowedToCreateVehicleResponse>> IsAllowedToCreateVehicleAsync(Guid userId);
    }
}
