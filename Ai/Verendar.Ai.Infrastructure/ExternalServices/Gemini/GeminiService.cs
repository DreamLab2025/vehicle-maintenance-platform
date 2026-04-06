using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verendar.Ai.Application.Dtos.Ai;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Enums;
using Verendar.Ai.Infrastructure.Configuration;
using Verendar.Common.Shared;

namespace Verendar.Ai.Infrastructure.ExternalServices
{
    public class GeminiService(
        IOptions<GeminiSettings> config,
        IHttpClientFactory httpClientFactory,
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
                        AiExternalServiceMessages.AiNotConfigured);
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
                        return ApiResponse<GenerativeAiResponse>.FailureResponse(AiExternalServiceMessages.AiRequestTimeout);
                    }
                    catch (HttpRequestException ex)
                    {
                        stopwatch.Stop();
                        logger.LogError(ex,
                            "Gemini API HTTP error - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ElapsedTime: {ElapsedTime}ms, Attempt: {Attempt}",
                            selectedModel, operation, userId, stopwatch.ElapsedMilliseconds, attempt + 1);

                        if (attempt >= _config.MaxRetries)
                            return ApiResponse<GenerativeAiResponse>.FailureResponse($"Network error: {ex.Message}");
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
                    return ApiResponse<GenerativeAiResponse>.FailureResponse(GetUserFriendlyMessage(response?.StatusCode));
                }

                if (string.IsNullOrEmpty(lastResponseContent))
                {
                    logger.LogWarning(
                        "Gemini API returned empty response - Model: {Model}, Operation: {Operation}, UserId: {UserId}",
                        selectedModel, operation, userId);
                    return ApiResponse<GenerativeAiResponse>.FailureResponse(AiExternalServiceMessages.EmptyAiResponse);
                }

                var geminiResponse = ParseGeminiResponse(lastResponseContent, selectedModel, stopwatch.ElapsedMilliseconds);

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
                    selectedModel, operation, userId, stopwatch.ElapsedMilliseconds, ex.GetType().Name);
                return ApiResponse<GenerativeAiResponse>.FailureResponse(AiExternalServiceMessages.UnexpectedAiError);
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
                Provider = AiProvider.Gemini,
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

        public async Task<ApiResponse<GenerativeAiResponse>> GenerateContentFromImageAsync(
            string imageUrl,
            string prompt,
            AiOperation operation,
            Guid userId,
            Guid? promptId = null,
            string? model = null,
            int? maxTokens = null,
            decimal? temperature = null,
            decimal? topP = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var selectedModel = model ?? _config.DefaultModel;

            try
            {
                if (string.IsNullOrWhiteSpace(_config.ApiKey))
                {
                    logger.LogError("Gemini API key is not configured");
                    return ApiResponse<GenerativeAiResponse>.FailureResponse(AiExternalServiceMessages.AiNotConfigured);
                }

                var imageFetch = await GenerativeAiImageHttp.FetchAsync(
                    httpClientFactory,
                    imageUrl,
                    Math.Min(_config.TimeoutSeconds, 120),
                    logger,
                    "Gemini vision",
                    cancellationToken);

                if (!imageFetch.IsSuccess)
                {
                    if (imageFetch.IsDownloadHttpFailure)
                        return ApiResponse<GenerativeAiResponse>.FailureResponse(AiExternalServiceMessages.ImageUrlDownloadFailed);
                    return ApiResponse<GenerativeAiResponse>.FailureResponse(AiExternalServiceMessages.ImageUrlInvalidOrUnreachable);
                }

                var imageBytes = imageFetch.Bytes!;
                var mimeType = imageFetch.MimeType;

                var httpClient = httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

                var base64Image = Convert.ToBase64String(imageBytes);

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { inline_data = new { mime_type = mimeType, data = base64Image } },
                                new { text = prompt }
                            }
                        }
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

                var url = $"{_config.ApiEndpoint.TrimEnd('/')}/models/{selectedModel}:generateContent";
                var jsonBody = JsonSerializer.Serialize(requestBody);

                logger.LogInformation(
                    "Calling Gemini multimodal API - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ImageSize: {ImageSize}b",
                    selectedModel, operation, userId, imageBytes.Length);

                HttpResponseMessage? response = null;
                var lastResponseContent = string.Empty;

                for (var attempt = 0; attempt <= _config.MaxRetries; attempt++)
                {
                    if (attempt > 0)
                    {
                        var delayMs = GetRetryDelayMs(response!, attempt);
                        logger.LogWarning(
                            "Gemini multimodal API retry {Attempt}/{MaxRetries} after {DelayMs}ms",
                            attempt, _config.MaxRetries, delayMs);
                        await Task.Delay(delayMs, cancellationToken);
                    }

                    using var request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Headers.TryAddWithoutValidation("X-goog-api-key", _config.ApiKey);
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    try
                    {
                        response = await httpClient.SendAsync(request, cancellationToken);
                        lastResponseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                        if (response.IsSuccessStatusCode)
                            break;

                        var isRetryable = response.StatusCode is System.Net.HttpStatusCode.TooManyRequests or System.Net.HttpStatusCode.ServiceUnavailable;
                        if (attempt >= _config.MaxRetries || !isRetryable)
                            break;
                    }
                    catch (TaskCanceledException ex)
                    {
                        stopwatch.Stop();
                        logger.LogError(ex, "Gemini multimodal API timeout - UserId: {UserId}", userId);
                        return ApiResponse<GenerativeAiResponse>.FailureResponse(AiExternalServiceMessages.AiRequestTimeout);
                    }
                    catch (HttpRequestException ex)
                    {
                        stopwatch.Stop();
                        logger.LogError(ex, "Gemini multimodal API HTTP error - UserId: {UserId}", userId);
                        if (attempt >= _config.MaxRetries)
                            return ApiResponse<GenerativeAiResponse>.FailureResponse($"Network error: {ex.Message}");
                        continue;
                    }
                }

                stopwatch.Stop();

                if (response == null || !response.IsSuccessStatusCode)
                {
                    logger.LogError(
                        "Gemini multimodal API error - Status: {StatusCode}, UserId: {UserId}, Response: {Response}",
                        response?.StatusCode, userId,
                        lastResponseContent?.Length > 500 ? lastResponseContent[..500] + "..." : lastResponseContent);
                    return ApiResponse<GenerativeAiResponse>.FailureResponse(GetUserFriendlyMessage(response?.StatusCode));
                }

                if (string.IsNullOrEmpty(lastResponseContent))
                    return ApiResponse<GenerativeAiResponse>.FailureResponse("AI service returned empty response");

                var geminiResponse = ParseGeminiResponse(lastResponseContent, selectedModel, stopwatch.ElapsedMilliseconds);

                logger.LogInformation(
                    "Gemini multimodal API success - Model: {Model}, Operation: {Operation}, UserId: {UserId}, Tokens: {TotalTokens}",
                    selectedModel, operation, userId, geminiResponse.TotalTokens);

                return ApiResponse<GenerativeAiResponse>.SuccessResponse(geminiResponse);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Gemini multimodal API unexpected error - UserId: {UserId}", userId);
                return ApiResponse<GenerativeAiResponse>.FailureResponse(AiExternalServiceMessages.UnexpectedAiError);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> CheckConnectivityAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_config.ApiKey))
                return (false, "Gemini API key is not configured");

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = "ping" } } } },
                generationConfig = new
                {
                    maxOutputTokens = 8,
                    temperature = 0,
                    responseMimeType = "application/json"
                }
            };

            try
            {
                var httpClient = httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(Math.Min(_config.TimeoutSeconds, 10));

                var url = $"{_config.ApiEndpoint.TrimEnd('/')}/models/{_config.DefaultModel}:generateContent";
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.TryAddWithoutValidation("X-goog-api-key", _config.ApiKey);
                request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(content))
                    return (true, null);

                return (false, GetUserFriendlyMessage(response.StatusCode) ?? content);
            }
            catch (TaskCanceledException)
            {
                return (false, "Request timeout");
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
