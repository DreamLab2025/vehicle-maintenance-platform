using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Options;
using Verendar.Ai.Application.Dtos.Ai;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Enums;
using Verendar.Ai.Infrastructure.Configuration;

namespace Verendar.Ai.Infrastructure.ExternalServices
{
    public class BedrockService(
        IOptions<BedrockSettings> config,
        IHttpClientFactory httpClientFactory,
        ILogger<BedrockService> logger
    ) : IGenerativeAiService
    {
        private readonly BedrockSettings _config = config.Value;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        private const int MaxVisionImageBytes = 20 * 1024 * 1024;

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
            var selectedModel = model ?? _config.DefaultModel;

            var configError = ValidateBedrockConfiguration();
            if (configError != null)
                return ApiResponse<GenerativeAiResponse>.FailureResponse(configError);

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

            ApplyInferenceParameters(requestBody, temperature, topP);

            return await InvokeBedrockModelAsync(
                requestBody,
                selectedModel,
                operation,
                userId,
                promptLengthForLog: prompt?.Length ?? 0,
                cancellationToken: default);
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
                var configError = ValidateBedrockConfiguration();
                if (configError != null)
                    return ApiResponse<GenerativeAiResponse>.FailureResponse(configError);

                var imageFetch = await GenerativeAiImageHttp.FetchAsync(
                    _httpClientFactory,
                    imageUrl,
                    Math.Min(_config.TimeoutSeconds, 120),
                    logger,
                    "Bedrock vision",
                    cancellationToken);

                if (!imageFetch.IsSuccess)
                {
                    if (imageFetch.IsDownloadHttpFailure)
                        return ApiResponse<GenerativeAiResponse>.FailureResponse(AiExternalServiceMessages.ImageUrlDownloadFailed);
                    return ApiResponse<GenerativeAiResponse>.FailureResponse(AiExternalServiceMessages.ImageUrlInvalidOrUnreachable);
                }

                var imageBytes = imageFetch.Bytes!;
                var mimeType = NormalizeAnthropicImageMediaType(imageFetch.MimeType);

                if (imageBytes.Length == 0)
                    return ApiResponse<GenerativeAiResponse>.FailureResponse("Ảnh tải về rỗng.");

                if (imageBytes.Length > MaxVisionImageBytes)
                {
                    logger.LogWarning("Bedrock vision: image too large ({Size} bytes)", imageBytes.Length);
                    return ApiResponse<GenerativeAiResponse>.FailureResponse("Ảnh vượt quá kích thước cho phép.");
                }

                var base64Image = Convert.ToBase64String(imageBytes);

                var requestBody = new Dictionary<string, object>
                {
                    ["anthropic_version"] = "bedrock-2023-05-31",
                    ["max_tokens"] = maxTokens ?? _config.DefaultParameters.MaxTokens,
                    ["messages"] = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["role"] = "user",
                            ["content"] = new object[]
                            {
                                new Dictionary<string, object>
                                {
                                    ["type"] = "image",
                                    ["source"] = new Dictionary<string, object>
                                    {
                                        ["type"] = "base64",
                                        ["media_type"] = mimeType,
                                        ["data"] = base64Image
                                    }
                                },
                                new Dictionary<string, object>
                                {
                                    ["type"] = "text",
                                    ["text"] = prompt
                                }
                            }
                        }
                    }
                };

                ApplyInferenceParameters(requestBody, temperature, topP);

                logger.LogInformation(
                    "Calling Bedrock vision API - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ImageBytes: {Size}, MediaType: {MediaType}",
                    selectedModel, operation, userId, imageBytes.Length, mimeType);

                return await InvokeBedrockModelAsync(
                    requestBody,
                    selectedModel,
                    operation,
                    userId,
                    promptLengthForLog: prompt?.Length ?? 0,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex,
                    "Bedrock vision unexpected error - Model: {Model}, Operation: {Operation}, UserId: {UserId}",
                    selectedModel, operation, userId);
                return ApiResponse<GenerativeAiResponse>.FailureResponse(AiExternalServiceMessages.UnexpectedAiError);
            }
        }

        private string? ValidateBedrockConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_config.AccessKey) || string.IsNullOrWhiteSpace(_config.SecretKey))
            {
                logger.LogError("Bedrock: AccessKey and SecretKey are required. Set Bedrock:AccessKey and Bedrock:SecretKey in User Secrets or appsettings.");
                return "AI service is not properly configured. Set Bedrock:AccessKey and Bedrock:SecretKey in User Secrets or configuration.";
            }

            if (string.IsNullOrWhiteSpace(_config.Region))
            {
                logger.LogError("Bedrock: Region is required");
                return "AI service is not properly configured (missing Region).";
            }

            return null;
        }

        private void ApplyInferenceParameters(
            Dictionary<string, object> requestBody,
            decimal? temperature,
            decimal? topP)
        {
            if (temperature.HasValue || _config.DefaultParameters.Temperature != 0)
                requestBody["temperature"] = (double)(temperature ?? _config.DefaultParameters.Temperature);
            if (topP.HasValue || _config.DefaultParameters.TopP != 0)
                requestBody["top_p"] = (double)(topP ?? _config.DefaultParameters.TopP);
            if (_config.DefaultParameters.TopK > 0)
                requestBody["top_k"] = _config.DefaultParameters.TopK;
        }

        private static string NormalizeAnthropicImageMediaType(string? mediaType)
        {
            if (string.IsNullOrWhiteSpace(mediaType))
                return "image/jpeg";

            var lower = mediaType.ToLowerInvariant();
            if (lower == "image/jpg")
                return "image/jpeg";
            if (lower is "image/jpeg" or "image/png" or "image/gif" or "image/webp")
                return lower;

            return "image/jpeg";
        }

        private async Task<ApiResponse<GenerativeAiResponse>> InvokeBedrockModelAsync(
            Dictionary<string, object> requestBody,
            string selectedModel,
            AiOperation operation,
            Guid userId,
            int promptLengthForLog,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var jsonBody = JsonSerializer.Serialize(requestBody);
                var bodyBytes = Encoding.UTF8.GetBytes(jsonBody);

                var credentials = new BasicAWSCredentials(_config.AccessKey!, _config.SecretKey!);
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
                    selectedModel, operation, userId, promptLengthForLog, requestBody["max_tokens"], requestBody.GetValueOrDefault("temperature"));

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
                        await Task.Delay(delayMs, cancellationToken);
                    }

                    try
                    {
                        invokeResponse = await client.InvokeModelAsync(request, cancellationToken);
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
                            return ApiResponse<GenerativeAiResponse>.FailureResponse(AiExternalServiceMessages.AiRequestTimeout);
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
                    responseContent = await reader.ReadToEndAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    logger.LogWarning(
                        "Bedrock API returned empty response - Model: {Model}, Operation: {Operation}, UserId: {UserId}",
                        selectedModel, operation, userId);
                    return ApiResponse<GenerativeAiResponse>.FailureResponse(AiExternalServiceMessages.EmptyAiResponse);
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
                return ApiResponse<GenerativeAiResponse>.FailureResponse(AiExternalServiceMessages.UnexpectedAiError);
            }
        }

        private GenerativeAiResponse ParseBedrockResponse(string responseContent, string model, long responseTimeMs)
        {
            var root = JsonDocument.Parse(responseContent).RootElement;

            var content = string.Empty;
            if (root.TryGetProperty("content", out var contentArr))
            {
                foreach (var block in contentArr.EnumerateArray())
                {
                    if (block.TryGetProperty("type", out var typeEl) &&
                        typeEl.GetString() == "text" &&
                        block.TryGetProperty("text", out var textProp))
                    {
                        content = textProp.GetString() ?? string.Empty;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(content) && contentArr.GetArrayLength() > 0)
                {
                    var first = contentArr[0];
                    if (first.TryGetProperty("text", out var textProp))
                        content = textProp.GetString() ?? string.Empty;
                }
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
                return AiExternalServiceMessages.AiRequestTimeout;
            if (error.Contains("end of its life", StringComparison.OrdinalIgnoreCase) ||
                error.Contains("deprecated", StringComparison.OrdinalIgnoreCase))
                return "Model Bedrock đã hết vòng đời. Vui lòng cập nhật Bedrock:DefaultModel lên phiên bản mới hơn.";
            if (error.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase) || error.Contains("UnrecognizedClientException", StringComparison.OrdinalIgnoreCase))
                return "Lỗi xác thực AWS. Kiểm tra cấu hình Bedrock.";
            return "Đã xảy ra lỗi khi gọi API AI. Vui lòng thử lại.";
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
