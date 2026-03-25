using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IGarageService
{
    Task<ApiResponse<GarageResponse>> CreateGarageAsync(Guid ownerId, GarageRequest request);
    Task<ApiResponse<List<GarageResponse>>> GetGaragesAsync(GarageFilterRequest request);
    Task<ApiResponse<GarageDetailResponse>> GetMyGarageAsync(Guid ownerId, CancellationToken ct = default);
    Task<ApiResponse<GarageDetailResponse>> GetGarageByIdAsync(Guid garageId, CancellationToken ct = default);
}
