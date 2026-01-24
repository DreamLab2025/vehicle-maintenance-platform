using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verendar.Ai.Application.Dtos.Ai;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Enums;
using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Ai.Infrastructure.Configuration;
using Verendar.Common.Shared;

namespace Verendar.Ai.Infrastructure.ExternalServices;

public class GeminiService(
    IOptions<GeminiSettings> config,
    IHttpClientFactory httpClientFactory,
    IUnitOfWork unitOfWork,
    ILogger<GeminiService> logger
) : IGenerativeAiService
{
    public Task<ApiResponse<GenerativeAiResponse>> GenerateContentAsync(
        string prompt,
        AiOperation operation,
        Guid userId,
        Guid? promptId = null,
        string? model = null,
        int? maxTokens = null,
        decimal? temperature = null,
        decimal? topP = null)
    {
        throw new NotImplementedException();
    }
}
