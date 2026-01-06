using VMP.Common.Shared;
using VMP.Vehicle.Application.Dtos;

namespace VMP.Vehicle.Application.Services.Interfaces
{
    public interface IModelImageService
    {
        Task<ApiResponse<List<ModelImageResponse>>> GetImagesByModelIdAsync(Guid vehicleModelId);
        Task<ApiResponse<ModelImageResponse>> GetImageByModelAndColorAsync(Guid vehicleModelId, string color);
        Task<ApiResponse<ModelImageResponse>> CreateImageAsync(ModelImageRequest request);
        Task<ApiResponse<ModelImageResponse>> UpdateImageAsync(Guid id, ModelImageUpdateRequest request);
        Task<ApiResponse<string>> DeleteImageAsync(Guid id);
    }
}
