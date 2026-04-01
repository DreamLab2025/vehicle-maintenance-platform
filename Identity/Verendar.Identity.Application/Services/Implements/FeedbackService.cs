using Verendar.Common.Shared;
using Verendar.Identity.Application.Mappings;
using Verendar.Identity.Application.Services.Interfaces;
using Verendar.Identity.Domain.Enums;
using Verendar.Identity.Domain.Repositories.Interfaces;

namespace Verendar.Identity.Application.Services.Implements;

public class FeedbackService(
    ILogger<FeedbackService> logger,
    IUnitOfWork unitOfWork) : IFeedbackService
{
    private readonly ILogger<FeedbackService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ApiResponse<FeedbackResponse>> SubmitAsync(
        Guid userId, CreateFeedbackRequest request, CancellationToken ct = default)
    {
        var feedback = request.ToEntity(userId);

        await _unitOfWork.Feedbacks.AddAsync(feedback);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Feedback submitted by user {UserId} with category {Category}", userId, request.Category);

        return ApiResponse<FeedbackResponse>.CreatedResponse(feedback.ToResponse(), "Gửi feedback thành công.");
    }

    public async Task<ApiResponse<List<FeedbackResponse>>> GetAllAsync(
        PaginationRequest pagination, CancellationToken ct = default)
    {
        pagination.Normalize();

        var result = await _unitOfWork.Feedbacks.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            null,
            q => q.OrderByDescending(f => f.CreatedAt));

        return ApiResponse<List<FeedbackResponse>>.SuccessPagedResponse(
            result.Items.Select(f => f.ToResponse()).ToList(),
            result.TotalCount,
            pagination.PageNumber,
            pagination.PageSize,
            "Lấy danh sách feedback thành công.");
    }

    public async Task<ApiResponse<FeedbackResponse>> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var feedback = await _unitOfWork.Feedbacks.GetByIdAsync(id);
        if (feedback is null)
            return ApiResponse<FeedbackResponse>.NotFoundResponse("Feedback không tồn tại.");

        return ApiResponse<FeedbackResponse>.SuccessResponse(feedback.ToResponse(), "Lấy feedback thành công.");
    }

    public async Task<ApiResponse<FeedbackResponse>> UpdateStatusAsync(
        Guid id, UpdateFeedbackStatusRequest request, CancellationToken ct = default)
    {
        var feedback = await _unitOfWork.Feedbacks.GetByIdAsync(id);
        if (feedback is null)
            return ApiResponse<FeedbackResponse>.NotFoundResponse("Feedback không tồn tại.");

        if (feedback.Status == FeedbackStatus.Resolved && request.Status == FeedbackStatus.Pending)
            return ApiResponse<FeedbackResponse>.FailureResponse(
                "Không thể đặt lại trạng thái Pending sau khi đã Resolved.", 400);

        feedback.Status = request.Status;

        await _unitOfWork.Feedbacks.UpdateAsync(feedback.Id, feedback);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Feedback {FeedbackId} status updated to {Status}", id, request.Status);

        return ApiResponse<FeedbackResponse>.SuccessResponse(feedback.ToResponse(), "Cập nhật trạng thái feedback thành công.");
    }
}
