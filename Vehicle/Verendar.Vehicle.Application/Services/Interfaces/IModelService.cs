namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IModelService
    {
        Task<ApiResponse<List<ModelSummary>>> GetAllModelsAsync(ModelFilterRequest filterRequest);
        Task<ApiResponse<ModelResponseWithVariants>> CreateModelAsync(ModelRequest request);
        Task<ApiResponse<ModelResponse>> UpdateModelAsync(Guid id, ModelRequest request);
        Task<ApiResponse<string>> DeleteModelAsync(Guid id);
        Task<ApiResponse<ModelResponseWithVariants>> GetModelByIdAsync(Guid id);
    }
}
