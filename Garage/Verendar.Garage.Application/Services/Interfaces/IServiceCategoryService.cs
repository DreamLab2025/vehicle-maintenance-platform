using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IServiceCategoryService
{
    Task<ApiResponse<List<ServiceCategoryResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<ServiceCategoryResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<ServiceCategoryResponse>> CreateAsync(CreateServiceCategoryRequest request, CancellationToken ct = default);
    Task<ApiResponse<ServiceCategoryResponse>> UpdateAsync(Guid id, UpdateServiceCategoryRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default);
}
