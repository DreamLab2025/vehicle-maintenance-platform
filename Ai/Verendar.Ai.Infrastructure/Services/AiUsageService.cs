using Microsoft.Extensions.Logging;
using Verendar.Ai.Application.Dtos.Ai;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Entities;
using Verendar.Ai.Domain.Enums;
using Verendar.Ai.Domain.Repositories.Interfaces;

namespace Verendar.Ai.Infrastructure.Services
{
    public class AiUsageService(
        IUnitOfWork unitOfWork,
        ILogger<AiUsageService> logger) : IAiUsageService
    {
        public async Task TrackSuccessAsync(
            Guid userId,
            AiOperation operation,
            Guid? promptId,
            GenerativeAiResponse response,
            string requestSummary,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var usage = new AiUsage
                {
                    UserId = userId,
                    Provider = response.Provider,
                    Model = response.Model,
                    Operation = operation,
                    InputTokens = response.InputTokens,
                    OutputTokens = response.OutputTokens,
                    TotalTokens = response.TotalTokens,
                    InputCost = response.InputCost,
                    OutputCost = response.OutputCost,
                    TotalCost = response.TotalCost,
                    ResponseTimeMs = response.ResponseTimeMs,
                    ErrorMessage = null,
                    RequestSummary = requestSummary?.Length > 500 ? requestSummary[..500] : requestSummary
                };

                await unitOfWork.AiUsages.AddAsync(usage);
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error tracking successful AI usage (Provider: {Provider}, Model: {Model})", response.Provider, response.Model);
            }
        }

        public async Task TrackFailedAsync(
            Guid userId,
            AiProvider provider,
            string model,
            AiOperation operation,
            Guid? promptId,
            long responseTimeMs,
            string errorMessage,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var usage = new AiUsage
                {
                    UserId = userId,
                    Provider = provider,
                    Model = model,
                    Operation = operation,
                    InputTokens = 0,
                    OutputTokens = 0,
                    TotalTokens = 0,
                    InputCost = 0,
                    OutputCost = 0,
                    TotalCost = 0,
                    ResponseTimeMs = (int)responseTimeMs,
                    ErrorMessage = errorMessage?.Length > 1000 ? errorMessage[..1000] : errorMessage,
                    RequestSummary = null
                };

                await unitOfWork.AiUsages.AddAsync(usage);
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error tracking failed AI usage (Provider: {Provider}, Model: {Model})", provider, model);
            }
        }
    }
}
