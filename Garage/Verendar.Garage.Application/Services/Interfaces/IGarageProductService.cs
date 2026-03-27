using Verendar.Common.Shared;
using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IGarageProductService
{
    Task<ApiResponse<List<GarageProductListItemResponse>>> GetProductsByBranchAsync(
        Guid branchId, bool activeOnly, PaginationRequest pagination, CancellationToken ct = default);

    Task<ApiResponse<GarageProductResponse>> GetProductByIdAsync(Guid id, CancellationToken ct = default);

    Task<ApiResponse<GarageProductResponse>> CreateProductAsync(
        Guid branchId, Guid requestingUserId, CreateGarageProductRequest request, CancellationToken ct = default);

    Task<ApiResponse<GarageProductResponse>> UpdateProductAsync(
        Guid id, Guid requestingUserId, UpdateGarageProductRequest request, CancellationToken ct = default);

    Task<ApiResponse<GarageProductResponse>> UpdateProductStatusAsync(
        Guid id, Guid requestingUserId, UpdateGarageProductStatusRequest request, CancellationToken ct = default);

    Task<ApiResponse<bool>> DeleteProductAsync(Guid id, Guid requestingUserId, CancellationToken ct = default);
}
