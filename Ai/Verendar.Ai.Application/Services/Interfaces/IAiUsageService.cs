namespace Verendar.Ai.Application.Services.Interfaces
{
    public interface IAiUsageService
    {
        Task TrackSuccessAsync(
            Guid userId,
            AiOperation operation,
            Guid? promptId,
            GenerativeAiResponse response,
            string requestSummary,
            CancellationToken cancellationToken = default);

        Task TrackFailedAsync(
            Guid userId,
            AiProvider provider,
            string model,
            AiOperation operation,
            Guid? promptId,
            long responseTimeMs,
            string errorMessage,
            CancellationToken cancellationToken = default);
    }
}
