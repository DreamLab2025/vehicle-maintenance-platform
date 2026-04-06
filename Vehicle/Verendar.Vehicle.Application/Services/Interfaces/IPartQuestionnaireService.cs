namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IPartQuestionnaireService
    {
        Task<ApiResponse<PartQuestionnaireResponse>> GetQuestionnaireByPartCategorySlugAsync(
            string partCategorySlug,
            CancellationToken cancellationToken = default);
    }
}
