using Verender.Common.Shared;
using Verender.Vehicle.Application.Dtos;

namespace Verender.Vehicle.Application.Services.Interfaces
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
