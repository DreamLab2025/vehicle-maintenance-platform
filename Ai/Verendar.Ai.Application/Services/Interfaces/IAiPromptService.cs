using Verendar.Ai.Application.Dtos.AiPrompt;

namespace Verendar.Ai.Application.Services.Interfaces;

public interface IAiPromptService
{
    Task<ApiResponse<AiPromptResponse>> GetPromptAsync(AiOperation operation, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<AiPromptResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<AiPromptResponse>> GetByOperationAsync(AiOperation operation, CancellationToken cancellationToken = default);
    Task<ApiResponse<AiPromptResponse>> UpdateAsync(AiOperation operation, UpdateAiPromptRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<AiPromptVersionResponse>>> GetVersionsAsync(AiOperation operation, CancellationToken cancellationToken = default);
    Task<ApiResponse<AiPromptResponse>> RollbackAsync(AiOperation operation, RollbackAiPromptRequest request, CancellationToken cancellationToken = default);
}
