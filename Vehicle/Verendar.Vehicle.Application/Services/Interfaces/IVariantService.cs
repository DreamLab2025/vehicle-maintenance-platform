namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IVariantService
    {
        Task<ApiResponse<List<VariantResponse>>> GetImagesByModelIdAsync(Guid vehicleModelId);
        Task<ApiResponse<VariantResponse>> CreateImageAsync(VariantRequest request);
        Task<ApiResponse<VariantResponse>> UpdateImageAsync(Guid id, VariantUpdateRequest request);
        Task<ApiResponse<string>> DeleteImageAsync(Guid id);
    }
}
