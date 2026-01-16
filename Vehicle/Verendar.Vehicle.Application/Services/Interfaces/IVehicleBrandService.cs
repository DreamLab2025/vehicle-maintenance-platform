using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;

namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IVehicleBrandService
    {
        Task<ApiResponse<List<BrandResponse>>> GetAllBrandsAsync(PaginationRequest paginationRequest);
        Task<ApiResponse<List<BrandResponse>>> GetBrandsByTypeIdAsync(Guid typeId);
        Task<ApiResponse<BrandResponse>> CreateBrandAsync(BrandRequest request);
        Task<ApiResponse<BrandResponse>> UpdateBrandAsync(Guid id, BrandRequest request);
        Task<ApiResponse<string>> DeleteBrandAsync(Guid id);
        Task<ApiResponse<BulkBrandResponse>> BulkCreateBrandsAsync(BulkBrandRequest request);
    }
}
