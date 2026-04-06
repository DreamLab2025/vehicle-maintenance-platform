namespace Verendar.Identity.Application.Services.Interfaces;

public interface IFeedbackService
{
    Task<ApiResponse<FeedbackResponse>> SubmitAsync(Guid userId, CreateFeedbackRequest request, CancellationToken ct = default);
    Task<ApiResponse<List<FeedbackResponse>>> GetAllAsync(PaginationRequest pagination, CancellationToken ct = default);
    Task<ApiResponse<FeedbackResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<FeedbackResponse>> UpdateStatusAsync(Guid id, UpdateFeedbackStatusRequest request, CancellationToken ct = default);
}
