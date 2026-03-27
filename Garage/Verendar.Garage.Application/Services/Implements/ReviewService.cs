using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Application.Services.Implements;

public class ReviewService(
    ILogger<ReviewService> logger,
    IUnitOfWork unitOfWork) : IReviewService
{
    private readonly ILogger<ReviewService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ApiResponse<GarageReviewResponse>> SubmitReviewAsync(
        Guid userId, Guid bookingId, CreateReviewRequest request, CancellationToken ct = default)
    {
        var booking = await _unitOfWork.Bookings.FindOneAsync(
            b => b.Id == bookingId && b.DeletedAt == null);
        if (booking is null)
            return ApiResponse<GarageReviewResponse>.NotFoundResponse("Không tìm thấy booking.");

        if (booking.UserId != userId)
            return ApiResponse<GarageReviewResponse>.ForbiddenResponse("Bạn không có quyền đánh giá booking này.");

        if (booking.Status != BookingStatus.Completed)
            return ApiResponse<GarageReviewResponse>.FailureResponse(
                "Chỉ có thể đánh giá sau khi booking đã hoàn tất.", 400);

        var existing = await _unitOfWork.Reviews.FindOneAsync(
            r => r.BookingId == bookingId && r.DeletedAt == null);
        if (existing is not null)
            return ApiResponse<GarageReviewResponse>.ConflictResponse("Booking này đã được đánh giá.");

        var review = new GarageReview
        {
            GarageBranchId = booking.GarageBranchId,
            BookingId = bookingId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        await _unitOfWork.Reviews.AddAsync(review);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Review submitted for booking {BookingId} by user {UserId}", bookingId, userId);

        return ApiResponse<GarageReviewResponse>.CreatedResponse(review.ToResponse(), "Đánh giá thành công.");
    }

    public async Task<ApiResponse<GarageReviewResponse>> GetByBookingAsync(
        Guid bookingId, CancellationToken ct = default)
    {
        var review = await _unitOfWork.Reviews.FindOneAsync(
            r => r.BookingId == bookingId && r.DeletedAt == null);
        if (review is null)
            return ApiResponse<GarageReviewResponse>.NotFoundResponse("Chưa có đánh giá cho booking này.");

        return ApiResponse<GarageReviewResponse>.SuccessResponse(review.ToResponse(), "Lấy đánh giá thành công.");
    }

    public async Task<ApiResponse<List<GarageReviewResponse>>> GetByBranchAsync(
        Guid branchId, PaginationRequest pagination, CancellationToken ct = default)
    {
        pagination.Normalize();

        var (items, totalCount) = await _unitOfWork.Reviews.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            filter: r => r.GarageBranchId == branchId && r.DeletedAt == null,
            orderBy: q => q.OrderByDescending(r => r.CreatedAt));

        return ApiResponse<List<GarageReviewResponse>>.SuccessPagedResponse(
            items.Select(r => r.ToResponse()).ToList(),
            totalCount,
            pagination.PageNumber,
            pagination.PageSize,
            "Lấy danh sách đánh giá thành công.");
    }
}
