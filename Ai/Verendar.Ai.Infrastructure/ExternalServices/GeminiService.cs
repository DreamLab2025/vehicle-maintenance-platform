using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verendar.Ai.Application.Dtos.Ai;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Entities;
using Verendar.Ai.Domain.Enums;
using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Ai.Infrastructure.Configuration;
using Verendar.Common.Shared;

namespace Verendar.Ai.Infrastructure.ExternalServices
{
    public class GeminiService(
        IOptions<GeminiSettings> config,
        IHttpClientFactory httpClientFactory,
        IUnitOfWork unitOfWork,
        ILogger<GeminiService> logger
    ) : IGenerativeAiService
    {
        private readonly GeminiSettings _config = config.Value;

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
            var stopwatch = Stopwatch.StartNew();
            var selectedModel = model ?? _config.DefaultModel;

            try
            {
                if (string.IsNullOrWhiteSpace(_config.ApiKey))
                {
                    logger.LogError("Gemini API key is not configured");
                    return ApiResponse<GenerativeAiResponse>.FailureResponse(
                        "AI service is not properly configured");
                }

                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    },
                    generationConfig = new
                    {
                        maxOutputTokens = maxTokens ?? _config.DefaultParameters.MaxTokens,
                        temperature = temperature ?? _config.DefaultParameters.Temperature,
                        topP = topP ?? _config.DefaultParameters.TopP,
                        topK = _config.DefaultParameters.TopK,
                        responseMimeType = "application/json"
                    }
                };

                var httpClient = httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

                var url = $"{_config.ApiEndpoint.TrimEnd('/')}/models/{selectedModel}:generateContent";
                var jsonBody = JsonSerializer.Serialize(requestBody);

                logger.LogInformation(
                    "Calling Gemini API - Model: {Model}, Operation: {Operation}, UserId: {UserId}, PromptLength: {PromptLength}, MaxTokens: {MaxTokens}, Temperature: {Temperature}",
                    selectedModel, operation, userId, prompt?.Length ?? 0, requestBody.generationConfig.maxOutputTokens, requestBody.generationConfig.temperature);

                HttpResponseMessage? response = null;
                var lastResponseContent = string.Empty;

                // Manual retry logic with exponential backoff
                for (var attempt = 0; attempt <= _config.MaxRetries; attempt++)
                {
                    if (attempt > 0)
                    {
                        var delayMs = GetRetryDelayMs(response!, attempt);
                        logger.LogWarning(
                            "Gemini API retry {Attempt}/{MaxRetries} after {DelayMs}ms - Model: {Model}, Operation: {Operation}, UserId: {UserId}",
                            attempt, _config.MaxRetries, delayMs, selectedModel, operation, userId);
                        await Task.Delay(delayMs);
                    }

                    using var request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Headers.TryAddWithoutValidation("X-goog-api-key", _config.ApiKey);
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    try
                    {
                        response = await httpClient.SendAsync(request);
                        lastResponseContent = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                            break;

                        var isRetryable = response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable;
                        if (attempt >= _config.MaxRetries || !isRetryable)
                            break;
                    }
                    catch (TaskCanceledException ex)
                    {
                        stopwatch.Stop();
                        var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                        logger.LogError(ex,
                            "Gemini API timeout - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ConfiguredTimeout: {ConfiguredTimeout}s, ElapsedTime: {ElapsedTime:F2}s, Attempt: {Attempt}",
                            selectedModel, operation, userId, _config.TimeoutSeconds, elapsedSeconds, attempt + 1);

                        var timeoutMessage = $"Request timeout after {elapsedSeconds:F2}s (configured: {_config.TimeoutSeconds}s)";
                        await TrackFailedUsageAsync(userId, selectedModel, operation, promptId, stopwatch.ElapsedMilliseconds, timeoutMessage);
                        return ApiResponse<GenerativeAiResponse>.FailureResponse("AI service request timeout");
                    }
                    catch (HttpRequestException ex)
                    {
                        stopwatch.Stop();
                        logger.LogError(ex,
                            "Gemini API HTTP error - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ElapsedTime: {ElapsedTime}ms, Attempt: {Attempt}",
                            selectedModel, operation, userId, stopwatch.ElapsedMilliseconds, attempt + 1);

                        if (attempt >= _config.MaxRetries)
                        {
                            await TrackFailedUsageAsync(userId, selectedModel, operation, promptId, stopwatch.ElapsedMilliseconds, $"HTTP error: {ex.Message}");
                            return ApiResponse<GenerativeAiResponse>.FailureResponse($"Network error: {ex.Message}");
                        }
                        // Continue to retry
                        continue;
                    }
                }

                stopwatch.Stop();

                if (response == null || !response.IsSuccessStatusCode)
                {
                    logger.LogError(
                        "Gemini API error - Status: {StatusCode}, Model: {Model}, Operation: {Operation}, UserId: {UserId}, ResponseTime: {ResponseTime}ms, Response: {Response}",
                        response?.StatusCode, selectedModel, operation, userId, stopwatch.ElapsedMilliseconds,
                        lastResponseContent?.Length > 1000 ? lastResponseContent[..1000] + "..." : lastResponseContent);

                    await TrackFailedUsageAsync(userId, selectedModel, operation, promptId, stopwatch.ElapsedMilliseconds, lastResponseContent ?? "Unknown error");

                    var userMessage = GetUserFriendlyMessage(response?.StatusCode);
                    return ApiResponse<GenerativeAiResponse>.FailureResponse(userMessage);
                }

                if (string.IsNullOrEmpty(lastResponseContent))
                {
                    logger.LogWarning(
                        "Gemini API returned empty response - Model: {Model}, Operation: {Operation}, UserId: {UserId}",
                        selectedModel, operation, userId);
                    await TrackFailedUsageAsync(userId, selectedModel, operation, promptId, stopwatch.ElapsedMilliseconds, "Empty response from API");
                    return ApiResponse<GenerativeAiResponse>.FailureResponse("AI service returned empty response");
                }

                var geminiResponse = ParseGeminiResponse(lastResponseContent, selectedModel, stopwatch.ElapsedMilliseconds);

                await TrackSuccessfulUsageAsync(userId, selectedModel, operation, promptId, geminiResponse, prompt ?? string.Empty);

                logger.LogInformation(
                    "Gemini API success - Model: {Model}, Operation: {Operation}, UserId: {UserId}, Tokens: {TotalTokens}, Cost: ${Cost:F4}, Time: {Time}ms",
                    selectedModel, operation, userId, geminiResponse.TotalTokens, geminiResponse.TotalCost, stopwatch.ElapsedMilliseconds);

                return ApiResponse<GenerativeAiResponse>.SuccessResponse(geminiResponse);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex,
                    "Gemini API unexpected error - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ElapsedTime: {ElapsedTime}ms, ExceptionType: {ExceptionType}",
                    model ?? _config.DefaultModel, operation, userId, stopwatch.ElapsedMilliseconds, ex.GetType().Name);

                await TrackFailedUsageAsync(userId, model ?? _config.DefaultModel, operation, promptId, stopwatch.ElapsedMilliseconds, ex.Message);

                return ApiResponse<GenerativeAiResponse>.FailureResponse("An unexpected error occurred while calling AI service");
            }
        }

        private GenerativeAiResponse ParseGeminiResponse(string responseContent, string model, long responseTimeMs)
        {
            var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;

            var content = root.GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            var usageMetadata = root.GetProperty("usageMetadata");
            var promptTokenCount = usageMetadata.GetProperty("promptTokenCount").GetInt32();
            var candidatesTokenCount = usageMetadata.GetProperty("candidatesTokenCount").GetInt32();
            var totalTokenCount = usageMetadata.GetProperty("totalTokenCount").GetInt32();

            var inputCost = (promptTokenCount / 1_000_000m) * _config.Pricing.InputCostPer1MTokens;
            var outputCost = (candidatesTokenCount / 1_000_000m) * _config.Pricing.OutputCostPer1MTokens;

            return new GenerativeAiResponse
            {
                Content = content,
                InputTokens = promptTokenCount,
                OutputTokens = candidatesTokenCount,
                TotalTokens = totalTokenCount,
                InputCost = inputCost,
                OutputCost = outputCost,
                TotalCost = inputCost + outputCost,
                ResponseTimeMs = (int)responseTimeMs,
                Model = model
            };
        }

        private static int GetRetryDelayMs(HttpResponseMessage response, int attempt)
        {
            if (response.Headers.RetryAfter?.Delta is { } delta)
                return (int)Math.Min(delta.TotalMilliseconds, 60_000);

            if (response.Headers.RetryAfter?.Date is { } date)
            {
                var ms = (date - DateTimeOffset.UtcNow).TotalMilliseconds;
                if (ms > 0) return (int)Math.Min(ms, 60_000);
            }

            var backoffMs = (int)(Math.Pow(2, attempt) * 1000);
            return Math.Min(backoffMs, 60_000);
        }

        private static string GetUserFriendlyMessage(HttpStatusCode? statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.TooManyRequests => "Tạm thời quá tải. Vui lòng thử lại sau vài phút.",
                HttpStatusCode.ServiceUnavailable => "Dịch vụ AI tạm thời bận. Vui lòng thử lại sau.",
                HttpStatusCode.Unauthorized => "Lỗi xác thực API. Kiểm tra cấu hình Gemini.",
                HttpStatusCode.BadRequest => "Yêu cầu không hợp lệ. Vui lòng kiểm tra dữ liệu gửi lên.",
                HttpStatusCode.NotFound => "Model hoặc endpoint không tồn tại. Kiểm tra cấu hình.",
                HttpStatusCode.RequestEntityTooLarge => "Nội dung gửi lên vượt quá giới hạn. Vui lòng rút gọn.",
                _ => statusCode is { } s && (int)s >= 500
                    ? "Dịch vụ AI gặp sự cố. Vui lòng thử lại sau."
                    : "Đã xảy ra lỗi khi gọi API AI. Vui lòng thử lại."
            };
        }

        private async Task TrackSuccessfulUsageAsync(
            Guid userId,
            string model,
            AiOperation operation,
            Guid? promptId,
            GenerativeAiResponse response,
            string requestSummary)
        {
            try
            {
                var usage = new AiUsage
                {
                    UserId = userId,
                    Provider = AiProvider.Gemini,
                    Model = model,
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
                logger.LogError(ex, "Error tracking successful AI usage");
            }
        }

        private async Task TrackFailedUsageAsync(
            Guid userId,
            string model,
            AiOperation operation,
            Guid? promptId,
            long responseTimeMs,
            string errorMessage)
        {
            try
            {
                var usage = new AiUsage
                {
                    UserId = userId,
                    Provider = AiProvider.Gemini,
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
                logger.LogError(ex, "Error tracking failed AI usage");
            }
        }
    }
}
