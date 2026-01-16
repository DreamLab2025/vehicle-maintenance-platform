using Verender.Common.Shared;
using Verender.Vehicle.Application.Dtos;

namespace Verender.Vehicle.Application.Services.Interfaces
{
    public interface IVehicleVariantService
    {
        Task<ApiResponse<List<VehicleVariantResponse>>> GetImagesByModelIdAsync(Guid vehicleModelId);
        Task<ApiResponse<VehicleVariantResponse>> CreateImageAsync(VehicleVariantRequest request);
        Task<ApiResponse<VehicleVariantResponse>> UpdateImageAsync(Guid id, VehicleVariantUpdateRequest request);
        Task<ApiResponse<string>> DeleteImageAsync(Guid id);
    }
}
