namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IPartProductService
    {
        Task<ApiResponse<PartProductResponse>> CreateProductAsync(PartProductRequest request);
        Task<ApiResponse<PartProductResponse>> UpdateProductAsync(Guid id, PartProductRequest request);
        Task<ApiResponse<string>> DeleteProductAsync(Guid id);
        Task<ApiResponse<List<PartProductSummary>>> GetProductsByCategoryAsync(Guid categoryId, PaginationRequest paginationRequest);
        Task<ApiResponse<PartProductResponse>> GetProductByIdAsync(Guid id);
    }
}
