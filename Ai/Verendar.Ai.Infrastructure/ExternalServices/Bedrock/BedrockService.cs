using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verendar.Ai.Application.Dtos.Ai;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Enums;
using Verendar.Ai.Infrastructure.Configuration;
using Verendar.Common.Shared;

namespace Verendar.Ai.Infrastructure.ExternalServices
{
    public class BedrockService(
        IOptions<BedrockSettings> config,
        ILogger<BedrockService> logger
    ) : IGenerativeAiService
    {
        private readonly BedrockSettings _config = config.Value;

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
                if (string.IsNullOrWhiteSpace(_config.AccessKey) || string.IsNullOrWhiteSpace(_config.SecretKey))
                {
                    logger.LogError("Bedrock: AccessKey and SecretKey are required. Set Bedrock:AccessKey and Bedrock:SecretKey in User Secrets or appsettings.");
                    return ApiResponse<GenerativeAiResponse>.FailureResponse(
                        "AI service is not properly configured. Set Bedrock:AccessKey and Bedrock:SecretKey in User Secrets or configuration.");
                }

                if (string.IsNullOrWhiteSpace(_config.Region))
                {
                    logger.LogError("Bedrock: Region is required");
                    return ApiResponse<GenerativeAiResponse>.FailureResponse("AI service is not properly configured (missing Region).");
                }

                var requestBody = new Dictionary<string, object>
                {
                    ["anthropic_version"] = "bedrock-2023-05-31",
                    ["max_tokens"] = maxTokens ?? _config.DefaultParameters.MaxTokens,
                    ["messages"] = new[]
                    {
                        new Dictionary<string, object>
                        {
                            ["role"] = "user",
                            ["content"] = new[] { new Dictionary<string, object> { ["type"] = "text", ["text"] = prompt } }
                        }
                    }
                };

                if (temperature.HasValue || _config.DefaultParameters.Temperature != 0)
                    requestBody["temperature"] = (double)(temperature ?? _config.DefaultParameters.Temperature);
                if (topP.HasValue || _config.DefaultParameters.TopP != 0)
                    requestBody["top_p"] = (double)(topP ?? _config.DefaultParameters.TopP);
                if (_config.DefaultParameters.TopK > 0)
                    requestBody["top_k"] = _config.DefaultParameters.TopK;

                var jsonBody = JsonSerializer.Serialize(requestBody);
                var bodyBytes = Encoding.UTF8.GetBytes(jsonBody);

                var credentials = new BasicAWSCredentials(_config.AccessKey, _config.SecretKey);
                using var client = new AmazonBedrockRuntimeClient(credentials, RegionEndpoint.GetBySystemName(_config.Region));

                var request = new InvokeModelRequest
                {
                    ModelId = selectedModel,
                    Body = new MemoryStream(bodyBytes),
                    ContentType = "application/json",
                    Accept = "application/json"
                };

                logger.LogInformation(
                    "Calling Bedrock API - Model: {Model}, Operation: {Operation}, UserId: {UserId}, PromptLength: {PromptLength}, MaxTokens: {MaxTokens}, Temperature: {Temperature}",
                    selectedModel, operation, userId, prompt?.Length ?? 0, requestBody["max_tokens"], requestBody.GetValueOrDefault("temperature"));

                InvokeModelResponse? invokeResponse = null;
                var lastError = string.Empty;

                for (var attempt = 0; attempt <= _config.MaxRetries; attempt++)
                {
                    if (attempt > 0)
                    {
                        var delayMs = Math.Min((int)Math.Pow(2, attempt) * 1000, 60_000);
                        logger.LogWarning(
                            "Bedrock API retry {Attempt}/{MaxRetries} after {DelayMs}ms - Model: {Model}, Operation: {Operation}, UserId: {UserId}",
                            attempt, _config.MaxRetries, delayMs, selectedModel, operation, userId);
                        await Task.Delay(delayMs);
                    }

                    try
                    {
                        invokeResponse = await client.InvokeModelAsync(request);
                        break;
                    }
                    catch (AmazonBedrockRuntimeException ex)
                    {
                        lastError = ex.Message;
                        if (ex.ErrorCode?.Contains("Timeout", StringComparison.OrdinalIgnoreCase) == true ||
                            ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
                        {
                            stopwatch.Stop();
                            logger.LogError(ex,
                                "Bedrock API timeout - Model: {Model}, Operation: {Operation}, UserId: {UserId}, Attempt: {Attempt}",
                                selectedModel, operation, userId, attempt + 1);
                            return ApiResponse<GenerativeAiResponse>.FailureResponse("AI service request timeout");
                        }
                        if (attempt >= _config.MaxRetries) break;
                        if (ex.StatusCode != System.Net.HttpStatusCode.TooManyRequests &&
                            ex.StatusCode != System.Net.HttpStatusCode.ServiceUnavailable)
                            break;
                        continue;
                    }
                }

                stopwatch.Stop();

                if (invokeResponse == null || invokeResponse.Body == null || invokeResponse.Body.Length == 0)
                {
                    logger.LogError(
                        "Bedrock API error - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ResponseTime: {ResponseTime}ms, Error: {Error}",
                        selectedModel, operation, userId, stopwatch.ElapsedMilliseconds, lastError);
                    return ApiResponse<GenerativeAiResponse>.FailureResponse(GetUserFriendlyMessage(lastError));
                }

                string responseContent;
                using (var reader = new StreamReader(invokeResponse.Body))
                    responseContent = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    logger.LogWarning(
                        "Bedrock API returned empty response - Model: {Model}, Operation: {Operation}, UserId: {UserId}",
                        selectedModel, operation, userId);
                    return ApiResponse<GenerativeAiResponse>.FailureResponse("AI service returned empty response");
                }

                var bedrockResponse = ParseBedrockResponse(responseContent, selectedModel, stopwatch.ElapsedMilliseconds);

                logger.LogInformation(
                    "Bedrock API success - Model: {Model}, Operation: {Operation}, UserId: {UserId}, Tokens: {TotalTokens}, Cost: ${Cost:F4}, Time: {Time}ms",
                    selectedModel, operation, userId, bedrockResponse.TotalTokens, bedrockResponse.TotalCost, stopwatch.ElapsedMilliseconds);

                return ApiResponse<GenerativeAiResponse>.SuccessResponse(bedrockResponse);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex,
                    "Bedrock API unexpected error - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ElapsedTime: {ElapsedTime}ms, ExceptionType: {ExceptionType}",
                    selectedModel, operation, userId, stopwatch.ElapsedMilliseconds, ex.GetType().Name);
                return ApiResponse<GenerativeAiResponse>.FailureResponse("An unexpected error occurred while calling AI service");
            }
        }

        private GenerativeAiResponse ParseBedrockResponse(string responseContent, string model, long responseTimeMs)
        {
            var root = JsonDocument.Parse(responseContent).RootElement;

            var content = string.Empty;
            if (root.TryGetProperty("content", out var contentArr) && contentArr.GetArrayLength() > 0)
            {
                var first = contentArr[0];
                if (first.TryGetProperty("text", out var textProp))
                    content = textProp.GetString() ?? string.Empty;
            }

            var inputTokens = 0;
            var outputTokens = 0;
            if (root.TryGetProperty("usage", out var usage))
            {
                if (usage.TryGetProperty("input_tokens", out var inProp))
                    inputTokens = inProp.GetInt32();
                if (usage.TryGetProperty("output_tokens", out var outProp))
                    outputTokens = outProp.GetInt32();
            }

            var totalTokens = inputTokens + outputTokens;
            var inputCost = (inputTokens / 1_000_000m) * _config.Pricing.InputCostPer1MTokens;
            var outputCost = (outputTokens / 1_000_000m) * _config.Pricing.OutputCostPer1MTokens;

            return new GenerativeAiResponse
            {
                Content = content,
                Provider = AiProvider.Bedrock,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                TotalTokens = totalTokens,
                InputCost = inputCost,
                OutputCost = outputCost,
                TotalCost = inputCost + outputCost,
                ResponseTimeMs = (int)responseTimeMs,
                Model = model
            };
        }

        private static string GetUserFriendlyMessage(string? error)
        {
            if (string.IsNullOrEmpty(error)) return "Đã xảy ra lỗi khi gọi API AI. Vui lòng thử lại.";
            if (error.Contains("Throttling", StringComparison.OrdinalIgnoreCase))
                return "Tạm thời quá tải. Vui lòng thử lại sau vài phút.";
            if (error.Contains("timeout", StringComparison.OrdinalIgnoreCase))
                return "AI service request timeout";
            if (error.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase) || error.Contains("UnrecognizedClientException", StringComparison.OrdinalIgnoreCase))
                return "Lỗi xác thực AWS. Kiểm tra cấu hình Bedrock.";
            return "Đã xảy ra lỗi khi gọi API AI. Vui lòng thử lại.";
        }

        public Task<ApiResponse<GenerativeAiResponse>> GenerateContentFromImageAsync(
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
            logger.LogWarning("GenerateContentFromImageAsync is not supported by Bedrock provider. Use Gemini for image tasks.");
            return Task.FromResult(ApiResponse<GenerativeAiResponse>.FailureResponse("Image analysis is not supported by the current AI provider. Please use Gemini."));
        }

        public async Task<(bool Success, string? ErrorMessage)> CheckConnectivityAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_config.AccessKey) || string.IsNullOrWhiteSpace(_config.SecretKey))
                return (false, "Bedrock: AccessKey and SecretKey are required. Set in User Secrets or appsettings.");
            if (string.IsNullOrWhiteSpace(_config.Region))
                return (false, "Bedrock: Region is required.");

            var requestBody = new Dictionary<string, object>
            {
                ["anthropic_version"] = "bedrock-2023-05-31",
                ["max_tokens"] = 8,
                ["messages"] = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["role"] = "user",
                        ["content"] = new[] { new Dictionary<string, object> { ["type"] = "text", ["text"] = "ping" } }
                    }
                }
            };

            try
            {
                var credentials = new BasicAWSCredentials(_config.AccessKey, _config.SecretKey);
                using var client = new AmazonBedrockRuntimeClient(credentials, RegionEndpoint.GetBySystemName(_config.Region));

                var bodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(requestBody));
                var request = new InvokeModelRequest
                {
                    ModelId = _config.DefaultModel,
                    Body = new MemoryStream(bodyBytes),
                    ContentType = "application/json",
                    Accept = "application/json"
                };

                var response = await client.InvokeModelAsync(request, cancellationToken);
                if (response?.Body != null && response.Body.Length > 0)
                    return (true, null);

                return (false, "Empty response from Bedrock");
            }
            catch (AmazonBedrockRuntimeException ex)
            {
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
