namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IBrandService
    {
        Task<ApiResponse<List<BrandSummary>>> GetAllBrandsAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<List<BrandSummary>>> GetBrandsByTypeIdAsync(Guid typeId);
        Task<ApiResponse<BrandResponse>> GetBrandByIdAsync(Guid id);
        Task<ApiResponse<BrandResponse>> CreateBrandAsync(BrandRequest request);
        Task<ApiResponse<BrandResponse>> UpdateBrandAsync(Guid id, BrandRequest request);
        Task<ApiResponse<string>> DeleteBrandAsync(Guid id);
    }
}
