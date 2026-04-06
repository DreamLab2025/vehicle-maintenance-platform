using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IReviewService
{
    Task<ApiResponse<GarageReviewResponse>> SubmitReviewAsync(
        Guid userId, Guid bookingId, CreateReviewRequest request, CancellationToken ct = default);

    Task<ApiResponse<GarageReviewResponse>> GetByBookingAsync(
        Guid bookingId, Guid viewerId, CancellationToken ct = default);

    Task<ApiResponse<List<GarageReviewResponse>>> GetByBranchAsync(
        Guid branchId, PaginationRequest pagination, CancellationToken ct = default);
}
