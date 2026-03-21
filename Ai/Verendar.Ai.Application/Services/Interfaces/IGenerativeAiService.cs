using Verendar.Ai.Domain.Enums;
namespace Verendar.Ai.Application.Services.Interfaces
{
    public interface IGenerativeAiService
    {
        Task<ApiResponse<GenerativeAiResponse>> GenerateContentAsync(
            string prompt,
            AiOperation operation,
            Guid userId,
            Guid? promptId = null,
            string? model = null,
            int? maxTokens = null,
            decimal? temperature = null,
            decimal? topP = null);
        Task<(bool Success, string? ErrorMessage)> CheckConnectivityAsync(CancellationToken cancellationToken = default);
    }
}
