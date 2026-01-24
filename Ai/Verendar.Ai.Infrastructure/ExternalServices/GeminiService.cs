using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verendar.Ai.Application.Dtos.Ai;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Entities;
using Verendar.Ai.Domain.Enums;
using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Ai.Infrastructure.Configuration;
using Verendar.Ai.Infrastructure.ExternalServices.Gemini;
using Verendar.Common.Shared;

namespace Verendar.Ai.Infrastructure.ExternalServices;

public class GeminiService(
    IOptions<GeminiSettings> config,
    IHttpClientFactory httpClientFactory,
    IUnitOfWork unitOfWork,
    ILogger<GeminiService> logger
) : IGenerativeAiService
{
    private readonly GeminiSettings _settings = config.Value;

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
        var selectedModel = model ?? _settings.DefaultModel;

        try
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                logger.LogError("Gemini API key is not configured");
                return ApiResponse<GenerativeAiResponse>.FailureResponse(
                    "AI service is not properly configured");
            }

            var request = new GeminiRequest
            {
                Contents = new List<GeminiContent>
                {
                    new()
                    {
                        Role = "user",
                        Parts = new List<GeminiPart>
                        {
                            new() { Text = prompt }
                        }
                    }
                },
                GenerationConfig = new GeminiGenerationConfig
                {
                    Temperature = temperature ?? _settings.DefaultParameters.Temperature,
                    TopP = topP ?? _settings.DefaultParameters.TopP,
                    TopK = _settings.DefaultParameters.TopK,
                    MaxOutputTokens = maxTokens ?? _settings.DefaultParameters.MaxTokens,
                    ResponseMimeType = "application/json"
                }
            };

            var httpClient = httpClientFactory.CreateClient("Gemini");

            var endpoint = $"{_settings.ApiEndpoint}/models/{selectedModel}:generateContent?key={_settings.ApiKey}";
            var promptLength = prompt?.Length ?? 0;
            var requestJson = JsonSerializer.Serialize(request);

            logger.LogInformation(
                "Calling Gemini API - Model: {Model}, Operation: {Operation}, UserId: {UserId}, PromptLength: {PromptLength}, MaxTokens: {MaxTokens}, Temperature: {Temperature}",
                selectedModel, operation, userId, promptLength, request.GenerationConfig.MaxOutputTokens, request.GenerationConfig.Temperature);

            logger.LogDebug(
                "Gemini API Request - Endpoint: {Endpoint}, Request: {Request}",
                endpoint.Replace(_settings.ApiKey, "***"), requestJson);

            HttpResponseMessage? httpResponse = null;
            string? responseContent = null;
            
            try
            {
                httpResponse = await httpClient.PostAsJsonAsync(endpoint, request);
                responseContent = await httpResponse.Content.ReadAsStringAsync();
                stopwatch.Stop();

                logger.LogDebug(
                    "Gemini API Response - Status: {StatusCode}, ResponseTime: {ResponseTime}ms, ContentLength: {ContentLength}",
                    httpResponse.StatusCode, stopwatch.ElapsedMilliseconds, responseContent?.Length ?? 0);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex,
                    "Gemini API request failed - Model: {Model}, Operation: {Operation}, UserId: {UserId}, Endpoint: {Endpoint}, ElapsedTime: {ElapsedTime}ms, ExceptionType: {ExceptionType}",
                    selectedModel, operation, userId, endpoint.Replace(_settings.ApiKey, "***"), stopwatch.ElapsedMilliseconds, ex.GetType().Name);
                throw;
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                var statusCode = httpResponse.StatusCode;
                var reasonPhrase = httpResponse.ReasonPhrase;
                
                logger.LogError(
                    "Gemini API error - Status: {StatusCode} ({ReasonPhrase}), Model: {Model}, Operation: {Operation}, UserId: {UserId}, ResponseTime: {ResponseTime}ms, Response: {Response}",
                    statusCode, reasonPhrase, selectedModel, operation, userId, stopwatch.ElapsedMilliseconds, 
                    responseContent?.Length > 1000 ? responseContent[..1000] + "..." : responseContent);

                GeminiErrorResponse? errorResponse = null;
                string errorMessage = "Unknown error from AI service";
                string? errorCode = null;
                string? errorStatus = null;

                try
                {
                    errorResponse = JsonSerializer.Deserialize<GeminiErrorResponse>(responseContent ?? string.Empty);
                    if (errorResponse?.Error != null)
                    {
                        errorMessage = errorResponse.Error.Message ?? errorMessage;
                        errorCode = errorResponse.Error.Code.ToString();
                        errorStatus = errorResponse.Error.Status;
                        
                        logger.LogError(
                            "Gemini API error details - Code: {ErrorCode}, Status: {ErrorStatus}, Message: {ErrorMessage}",
                            errorCode, errorStatus, errorMessage);
                    }
                }
                catch (JsonException jsonEx)
                {
                    logger.LogWarning(jsonEx,
                        "Failed to deserialize Gemini error response - Response: {Response}",
                        responseContent?.Length > 500 ? responseContent[..500] : responseContent);
                }

                var fullErrorMessage = $"HTTP {statusCode}: {errorMessage}";
                if (!string.IsNullOrEmpty(errorCode))
                {
                    fullErrorMessage = $"[{errorCode}] {fullErrorMessage}";
                }

                await TrackUsageAsync(userId, operation, selectedModel, 0, 0, 0, stopwatch.ElapsedMilliseconds, fullErrorMessage);

                return ApiResponse<GenerativeAiResponse>.FailureResponse(
                    $"AI service error: {errorMessage}");
            }

            if (string.IsNullOrEmpty(responseContent))
            {
                logger.LogWarning(
                    "Gemini API returned empty response - Model: {Model}, Operation: {Operation}, UserId: {UserId}",
                    selectedModel, operation, userId);
                await TrackUsageAsync(userId, operation, selectedModel, 0, 0, 0, stopwatch.ElapsedMilliseconds, "Empty response from API");
                return ApiResponse<GenerativeAiResponse>.FailureResponse("AI service returned empty response");
            }

            GeminiResponse? geminiResponse = null;
            
            try
            {
                geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);
            }
            catch (JsonException jsonEx)
            {
                logger.LogError(jsonEx,
                    "Failed to deserialize Gemini response - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ResponseLength: {ResponseLength}, ResponsePreview: {ResponsePreview}",
                    selectedModel, operation, userId, responseContent?.Length ?? 0,
                    responseContent?.Length > 500 ? responseContent[..500] : responseContent);
                
                await TrackUsageAsync(userId, operation, selectedModel, 0, 0, 0, stopwatch.ElapsedMilliseconds, 
                    $"Failed to deserialize response: {jsonEx.Message}");
                return ApiResponse<GenerativeAiResponse>.FailureResponse("AI service returned invalid response format");
            }

            if (geminiResponse == null)
            {
                logger.LogWarning(
                    "Gemini API returned null response - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ResponseContent: {ResponseContent}",
                    selectedModel, operation, userId, responseContent?.Length > 500 ? responseContent[..500] : responseContent);
                await TrackUsageAsync(userId, operation, selectedModel, 0, 0, 0, stopwatch.ElapsedMilliseconds, "Null response from API");
                return ApiResponse<GenerativeAiResponse>.FailureResponse("AI service returned null response");
            }

            if (geminiResponse.Candidates == null || geminiResponse.Candidates.Count == 0)
            {
                logger.LogWarning(
                    "Gemini API returned no candidates - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ResponseTime: {ResponseTime}ms, ResponseContent: {ResponseContent}",
                    selectedModel, operation, userId, stopwatch.ElapsedMilliseconds,
                    responseContent?.Length > 500 ? responseContent[..500] : responseContent);
                await TrackUsageAsync(userId, operation, selectedModel, 0, 0, 0, stopwatch.ElapsedMilliseconds, "No candidates returned");
                return ApiResponse<GenerativeAiResponse>.FailureResponse("AI service returned no response");
            }

            var candidate = geminiResponse.Candidates[0];
            var content = candidate.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;

            var inputTokens = geminiResponse.UsageMetadata?.PromptTokenCount ?? 0;
            var outputTokens = geminiResponse.UsageMetadata?.CandidatesTokenCount ?? 0;
            var totalTokens = geminiResponse.UsageMetadata?.TotalTokenCount ?? (inputTokens + outputTokens);

            var inputCost = (inputTokens / 1_000_000m) * _settings.Pricing.InputCostPer1MTokens;
            var outputCost = (outputTokens / 1_000_000m) * _settings.Pricing.OutputCostPer1MTokens;
            var totalCost = inputCost + outputCost;

            await TrackUsageAsync(userId, operation, selectedModel, inputTokens, outputTokens, totalCost, stopwatch.ElapsedMilliseconds, null, prompt);

            var response = new GenerativeAiResponse
            {
                Content = content,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                TotalTokens = totalTokens,
                InputCost = inputCost,
                OutputCost = outputCost,
                TotalCost = totalCost,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                Model = selectedModel
            };

            logger.LogInformation(
                "Gemini API success - Tokens: {TotalTokens}, Cost: ${Cost:F4}, Time: {Time}ms",
                totalTokens, totalCost, stopwatch.ElapsedMilliseconds);

            return ApiResponse<GenerativeAiResponse>.SuccessResponse(response);
        }
        catch (TaskCanceledException ex)
        {
            stopwatch.Stop();
            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            logger.LogError(ex,
                "Gemini API timeout - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ConfiguredTimeout: {ConfiguredTimeout}s, ElapsedTime: {ElapsedTime:F2}s, InnerException: {InnerException}",
                selectedModel, operation, userId, _settings.TimeoutSeconds, elapsedSeconds, ex.InnerException?.GetType().Name ?? "None");
            
            var timeoutMessage = $"Request timeout after {elapsedSeconds:F2}s (configured: {_settings.TimeoutSeconds}s)";
            await TrackUsageAsync(userId, operation, selectedModel, 0, 0, 0, stopwatch.ElapsedMilliseconds, timeoutMessage);
            return ApiResponse<GenerativeAiResponse>.FailureResponse("AI service request timeout");
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            logger.LogError(ex,
                "Gemini API HTTP error - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ElapsedTime: {ElapsedTime}ms, StatusCode: {StatusCode}, Message: {Message}",
                selectedModel, operation, userId, stopwatch.ElapsedMilliseconds, 
                ex.Data.Contains("StatusCode") ? ex.Data["StatusCode"] : "Unknown", ex.Message);
            
            await TrackUsageAsync(userId, operation, selectedModel, 0, 0, 0, stopwatch.ElapsedMilliseconds, 
                $"HTTP error: {ex.Message}");
            return ApiResponse<GenerativeAiResponse>.FailureResponse($"Network error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            stopwatch.Stop();
            logger.LogError(ex,
                "Gemini API JSON deserialization error - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ElapsedTime: {ElapsedTime}ms, Path: {Path}",
                selectedModel, operation, userId, stopwatch.ElapsedMilliseconds, ex.Path ?? "Unknown");
            
            await TrackUsageAsync(userId, operation, selectedModel, 0, 0, 0, stopwatch.ElapsedMilliseconds, 
                $"JSON error: {ex.Message}");
            return ApiResponse<GenerativeAiResponse>.FailureResponse("Failed to parse AI service response");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex,
                "Gemini API unexpected error - Model: {Model}, Operation: {Operation}, UserId: {UserId}, ElapsedTime: {ElapsedTime}ms, ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                selectedModel, operation, userId, stopwatch.ElapsedMilliseconds, ex.GetType().Name, ex.Message, ex.StackTrace);
            
            await TrackUsageAsync(userId, operation, selectedModel, 0, 0, 0, stopwatch.ElapsedMilliseconds, 
                $"{ex.GetType().Name}: {ex.Message}");
            return ApiResponse<GenerativeAiResponse>.FailureResponse($"An unexpected error occurred: {ex.Message}");
        }
    }

    private async Task TrackUsageAsync(
        Guid userId,
        AiOperation operation,
        string model,
        int inputTokens,
        int outputTokens,
        decimal totalCost,
        long responseTimeMs,
        string? errorMessage,
        string? requestSummary = null)
    {
        try
        {
            var usage = new AiUsage
            {
                UserId = userId,
                Provider = AiProvider.Gemini,
                Model = model,
                Operation = operation,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                TotalTokens = inputTokens + outputTokens,
                InputCost = (inputTokens / 1_000_000m) * _settings.Pricing.InputCostPer1MTokens,
                OutputCost = (outputTokens / 1_000_000m) * _settings.Pricing.OutputCostPer1MTokens,
                TotalCost = totalCost,
                ResponseTimeMs = (int)responseTimeMs,
                ErrorMessage = errorMessage,
                RequestSummary = requestSummary?.Length > 500 ? requestSummary[..500] : requestSummary
            };

            await unitOfWork.AiUsages.AddAsync(usage);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error tracking AI usage");
        }
    }
}
