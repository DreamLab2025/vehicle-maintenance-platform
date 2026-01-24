using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;
using Verendar.Common.Shared;

namespace Verendar.Ai.Application.Services.Interfaces;

public interface IVehicleMaintenanceAnalysisService
{
    Task<ApiResponse<VehicleQuestionnaireResponse>> AnalyzeQuestionnaireAsync(
        VehicleQuestionnaireRequest request,
        CancellationToken cancellationToken = default);
}
