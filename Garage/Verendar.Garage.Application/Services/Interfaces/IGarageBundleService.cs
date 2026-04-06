using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IGarageBundleService
{
    Task<ApiResponse<List<GarageBundleListItemResponse>>> GetBundlesByBranchAsync(
        GarageBundleQueryRequest query, CancellationToken ct = default);

    Task<ApiResponse<GarageBundleResponse>> GetBundleByIdAsync(Guid id, CancellationToken ct = default);

    Task<ApiResponse<GarageBundleResponse>> CreateBundleAsync(
        Guid branchId, Guid requestingUserId, CreateGarageBundleRequest request, CancellationToken ct = default);

    Task<ApiResponse<GarageBundleResponse>> UpdateBundleAsync(
        Guid id, Guid requestingUserId, UpdateGarageBundleRequest request, CancellationToken ct = default);

    Task<ApiResponse<GarageBundleResponse>> UpdateBundleStatusAsync(
        Guid id, Guid requestingUserId, UpdateGarageBundleStatusRequest request, CancellationToken ct = default);

    Task<ApiResponse<bool>> DeleteBundleAsync(Guid id, Guid requestingUserId, CancellationToken ct = default);
}
