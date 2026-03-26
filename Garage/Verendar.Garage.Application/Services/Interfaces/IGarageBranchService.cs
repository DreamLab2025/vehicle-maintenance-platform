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

    Task<ApiResponse<List<BranchMapItemResponse>>> GetBranchesForMapAsync(
        BranchMapSearchRequest request,
        CancellationToken ct = default);
}
