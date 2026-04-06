using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Application.Services.Implements;

public class ReviewService(
    ILogger<ReviewService> logger,
    IUnitOfWork unitOfWork,
    IBookingService bookingService) : IReviewService
{
    private readonly ILogger<ReviewService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IBookingService _bookingService = bookingService;

    public async Task<ApiResponse<GarageReviewResponse>> SubmitReviewAsync(
        Guid userId, Guid bookingId, CreateReviewRequest request, CancellationToken ct = default)
    {
        var booking = await _unitOfWork.Bookings.FindOneAsync(
            b => b.Id == bookingId && b.DeletedAt == null);
        if (booking is null)
            return ApiResponse<GarageReviewResponse>.NotFoundResponse(EndpointMessages.Review.BookingNotFound);

        if (booking.UserId != userId)
            return ApiResponse<GarageReviewResponse>.ForbiddenResponse(EndpointMessages.Review.ReviewForbidden);

        if (booking.Status != BookingStatus.Completed)
            return ApiResponse<GarageReviewResponse>.FailureResponse(
                EndpointMessages.Review.BookingNotCompleted, 400);

        var existing = await _unitOfWork.Reviews.FindOneAsync(
            r => r.BookingId == bookingId && r.DeletedAt == null);
        if (existing is not null)
            return ApiResponse<GarageReviewResponse>.ConflictResponse(EndpointMessages.Review.AlreadyReviewed);

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

        return ApiResponse<GarageReviewResponse>.CreatedResponse(review.ToResponse(), EndpointMessages.Review.SubmitSuccess);
    }

    public async Task<ApiResponse<GarageReviewResponse>> GetByBookingAsync(
        Guid bookingId, Guid viewerId, CancellationToken ct = default)
    {
        var review = await _unitOfWork.Reviews.FindOneAsync(
            r => r.BookingId == bookingId && r.DeletedAt == null);
        if (review is null)
            return ApiResponse<GarageReviewResponse>.NotFoundResponse(EndpointMessages.Review.ReviewNotFound);

        if (!await _bookingService.CanViewBookingAsync(bookingId, viewerId, ct))
            return ApiResponse<GarageReviewResponse>.ForbiddenResponse(EndpointMessages.Booking.BookingForbiddenView);

        return ApiResponse<GarageReviewResponse>.SuccessResponse(review.ToResponse(), EndpointMessages.Review.GetReviewSuccess);
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
            EndpointMessages.Review.GetBranchReviewsSuccess);
    }
}
