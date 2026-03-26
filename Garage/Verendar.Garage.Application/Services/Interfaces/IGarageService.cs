using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IGarageService
{
    Task<ApiResponse<GarageResponse>> CreateGarageAsync(Guid ownerId, GarageRequest request);
    Task<ApiResponse<List<GarageResponse>>> GetGaragesAsync(GarageFilterRequest request);
    Task<ApiResponse<GarageDetailResponse>> GetMyGarageAsync(Guid ownerId, CancellationToken ct = default);
    Task<ApiResponse<GarageDetailResponse>> GetGarageByIdAsync(Guid garageId, CancellationToken ct = default);
    Task<ApiResponse<GarageResponse>> UpdateGarageStatusAsync(Guid garageId, UpdateGarageStatusRequest request, Guid adminUserId, CancellationToken ct = default);
    Task<ApiResponse<GarageResponse>> UpdateGarageInfoAsync(Guid garageId, Guid ownerId, GarageRequest request, CancellationToken ct = default);
    Task<ApiResponse<GarageResponse>> ResubmitGarageAsync(Guid garageId, Guid ownerId, CancellationToken ct = default);
}
