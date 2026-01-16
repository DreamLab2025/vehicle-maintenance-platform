using Verender.Common.Shared;
using Verender.Vehicle.Application.Dtos;

namespace Verender.Vehicle.Application.Services.Interfaces
{
    public interface IVehicleModelService
    {
        Task<ApiResponse<List<ModelResponseWithVariants>>> GetAllModelsAsync(ModelFilterRequest filterRequest);
        Task<ApiResponse<ModelResponseWithVariants>> CreateModelAsync(ModelRequest request);
        Task<ApiResponse<ModelResponse>> UpdateModelAsync(Guid id, ModelRequest request);
        Task<ApiResponse<string>> DeleteModelAsync(Guid id);
        Task<ApiResponse<BulkModelResponse>> BulkCreateModelsAsync(BulkModelRequest request);
        Task<ApiResponse<BulkModelResponse>> BulkCreateModelsFromFileAsync(BulkModelFileRequest request);
        Task<ApiResponse<ModelResponseWithVariants>> GetModelByIdAsync(Guid id);
    }
}
