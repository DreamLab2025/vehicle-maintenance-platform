using System.Diagnostics;
using Verendar.Ai.Application.Dtos.Ai;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Enums;
using Verendar.Common.Shared;

namespace Verendar.Ai.Infrastructure.ExternalServices
{
    public class AiUsageTrackingDecorator(
        IGenerativeAiService inner,
        IAiUsageService usageService,
        AiProvider provider) : IGenerativeAiService
    {
        public async Task<ApiResponse<GenerativeAiResponse>> GenerateContentAsync(
            string prompt,
            AiOperation operation,
            Guid userId,
            Guid? promptId = null,
            string? model = null,
            int? maxTokens = null,
            decimal? temperature = null,
            decimal? topP = null)
        {
            var sw = Stopwatch.StartNew();
            var result = await inner.GenerateContentAsync(prompt, operation, userId, promptId, model, maxTokens, temperature, topP);
            sw.Stop();

            if (result.IsSuccess && result.Data != null)
                await usageService.TrackSuccessAsync(userId, operation, promptId, result.Data, prompt);
            else
                await usageService.TrackFailedAsync(userId, provider, model ?? "default", operation, promptId, sw.ElapsedMilliseconds, result.Message ?? "Unknown error");

            return result;
        }

        public Task<(bool Success, string? ErrorMessage)> CheckConnectivityAsync(CancellationToken cancellationToken = default) =>
            inner.CheckConnectivityAsync(cancellationToken);
    }
}
