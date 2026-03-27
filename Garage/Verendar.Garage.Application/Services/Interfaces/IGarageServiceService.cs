using Verendar.Common.Shared;
using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IGarageServiceService
{
    Task<ApiResponse<List<GarageServiceListItemResponse>>> GetServicesByBranchAsync(
        Guid branchId, bool activeOnly, PaginationRequest pagination, CancellationToken ct = default);

    Task<ApiResponse<GarageServiceResponse>> GetServiceByIdAsync(Guid id, CancellationToken ct = default);

    Task<ApiResponse<GarageServiceResponse>> CreateServiceAsync(
        Guid branchId, Guid requestingUserId, CreateGarageServiceRequest request, CancellationToken ct = default);

    Task<ApiResponse<GarageServiceResponse>> UpdateServiceAsync(
        Guid id, Guid requestingUserId, UpdateGarageServiceRequest request, CancellationToken ct = default);

    Task<ApiResponse<GarageServiceResponse>> UpdateServiceStatusAsync(
        Guid id, Guid requestingUserId, UpdateGarageServiceStatusRequest request, CancellationToken ct = default);

    Task<ApiResponse<bool>> DeleteServiceAsync(Guid id, Guid requestingUserId, CancellationToken ct = default);
}
