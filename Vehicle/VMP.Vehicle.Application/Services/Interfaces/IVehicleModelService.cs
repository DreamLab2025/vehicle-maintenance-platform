using VMP.Common.Shared;
using VMP.Vehicle.Application.Dtos;

namespace VMP.Vehicle.Application.Services.Interfaces
{
    public interface IVehicleModelService
    {
        Task<ApiResponse<List<ModelResponse>>> GetAllModelsAsync(ModelFilterRequest filterRequest);
        Task<ApiResponse<ModelResponse>> CreateModelAsync(ModelRequest request);
        Task<ApiResponse<ModelResponse>> UpdateModelAsync(Guid id, ModelRequest request);
        Task<ApiResponse<string>> DeleteModelAsync(Guid id);
        Task<ApiResponse<BulkModelResponse>> BulkCreateModelsAsync(BulkModelRequest request);
        Task<ApiResponse<BulkModelResponse>> BulkCreateModelsFromFileAsync(BulkModelFileRequest request);
    }
}
