using Verendar.Vehicle.Application.Mappings;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Application.Services.Implements
{
    public class PartQuestionnaireService(
        ILogger<PartQuestionnaireService> logger,
        IUnitOfWork unitOfWork) : IPartQuestionnaireService
    {
        private readonly ILogger<PartQuestionnaireService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ApiResponse<PartQuestionnaireResponse>> GetQuestionnaireByPartCategorySlugAsync(
            string partCategorySlug,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(partCategorySlug))
                return ApiResponse<PartQuestionnaireResponse>.FailureResponse("Mã danh mục phụ tùng không hợp lệ");

            var category = await _unitOfWork.PartCategories.GetBySlugAsync(partCategorySlug.Trim(), cancellationToken);
            if (category == null)
            {
                _logger.LogWarning("GetQuestionnaire: part category slug not found {Slug}", partCategorySlug);
                return ApiResponse<PartQuestionnaireResponse>.NotFoundResponse("Không tìm thấy danh mục phụ tùng");
            }

            var questions = await _unitOfWork.MaintenanceQuestions.GetActiveForPartCategoryAsync(
                category.Id,
                cancellationToken);

            if (questions.Count == 0)
            {
                _logger.LogWarning("GetQuestionnaire: no questions for part {Slug}", partCategorySlug);
                return ApiResponse<PartQuestionnaireResponse>.NotFoundResponse("Chưa có bộ câu hỏi cho danh mục này");
            }

            return ApiResponse<PartQuestionnaireResponse>.SuccessResponse(
                category.ToQuestionnaireResponse(questions));
        }
    }
}
