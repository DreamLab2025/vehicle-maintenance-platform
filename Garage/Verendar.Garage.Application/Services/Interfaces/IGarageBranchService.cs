using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IGarageBranchService
{
    Task<ApiResponse<GarageBranchResponse>> CreateBranchAsync(
        Guid garageId,
        Guid requestingUserId,
        GarageBranchRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<GarageBranchResponse>> GetBranchByIdAsync(
        Guid garageId,
        Guid branchId,
        CancellationToken ct = default);

    Task<ApiResponse<GarageBranchResponse>> GetMyBranchAsync(Guid userId, CancellationToken ct = default);

    Task<ApiResponse<List<GarageBranchSummaryResponse>>> GetBranchesAsync(
        Guid garageId,
        PaginationRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<GarageBranchResponse>> UpdateBranchAsync(
        Guid garageId,
        Guid branchId,
        Guid requestingUserId,
        GarageBranchRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<bool>> DeleteBranchAsync(
        Guid garageId,
        Guid branchId,
        Guid requestingUserId,
        CancellationToken ct = default);

    Task<ApiResponse<GarageBranchResponse>> UpdateBranchStatusAsync(
        Guid garageId,
        Guid branchId,
        Guid requestingUserId,
        UpdateBranchStatusRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<List<BranchMapItemResponse>>> GetBranchesForMapAsync(
        BranchMapSearchRequest request,
        CancellationToken ct = default);
}
