using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;
namespace Verendar.Ai.Application.Services.Interfaces
{
    public interface IVehicleMaintenanceAnalysisService
    {
        Task<ApiResponse<VehicleQuestionnaireResponse>> AnalyzeQuestionnaireAsync(
            Guid userId,
            VehicleQuestionnaireRequest request,
            CancellationToken cancellationToken = default);
    }
}
