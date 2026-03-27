using Verendar.Ai.Application.Dtos.AiPrompt;
using Verendar.Ai.Application.Mappings;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Entities;
using Verendar.Ai.Domain.Enums;
using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Common.Caching;
using Verendar.Common.Shared;

namespace Verendar.Ai.Application.Services.Implements;

public class AiPromptService(
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<AiPromptService> logger) : IAiPromptService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICacheService _cacheService = cacheService;
    private readonly ILogger<AiPromptService> _logger = logger;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    private static readonly Dictionary<AiOperation, string[]> RequiredPlaceholders = new()
    {
        [AiOperation.AnalyzeMaintenanceQuestionnaire] =
        [
            "[[TODAY]]",
            "[[VEHICLE_NAME]]",
            "[[CURRENT_ODO]]",
            "[[PURCHASE_DATE]]",
            "[[SCHEDULE_BLOCK]]",
            "[[ANSWER_BLOCK]]",
            "[[PART_CATEGORY_SLUG]]",
        ],
    };

    private static string CacheKey(AiOperation operation) => $"ai:prompt:{(int)operation}";

    public async Task<ApiResponse<AiPromptResponse>> GetPromptAsync(AiOperation operation, CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<AiPromptResponse>(CacheKey(operation));
        if (cached != null)
            return ApiResponse<AiPromptResponse>.SuccessResponse(cached);

        var entity = await _unitOfWork.AiPrompts.GetByOperationAsync(operation, cancellationToken);
        if (entity == null)
            return ApiResponse<AiPromptResponse>.NotFoundResponse($"Không tìm thấy prompt cho operation {operation}");

        var response = entity.ToResponse();
        await _cacheService.SetAsync(CacheKey(operation), response, CacheTtl);

        return ApiResponse<AiPromptResponse>.SuccessResponse(response);
    }

    public async Task<ApiResponse<List<AiPromptResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.AiPrompts.GetAllPromptsAsync(cancellationToken);
        var responses = entities.Select(e => e.ToResponse()).ToList();
        return ApiResponse<List<AiPromptResponse>>.SuccessResponse(responses);
    }

    public async Task<ApiResponse<AiPromptResponse>> GetByOperationAsync(AiOperation operation, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.AiPrompts.GetByOperationAsync(operation, cancellationToken);
        if (entity == null)
            return ApiResponse<AiPromptResponse>.NotFoundResponse($"Không tìm thấy prompt cho operation {operation}");

        return ApiResponse<AiPromptResponse>.SuccessResponse(entity.ToResponse());
    }

    public async Task<ApiResponse<AiPromptResponse>> UpdateAsync(
        AiOperation operation,
        UpdateAiPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.AiPrompts.GetByOperationAsync(operation, cancellationToken);
        if (entity == null)
            return ApiResponse<AiPromptResponse>.NotFoundResponse($"Không tìm thấy prompt cho operation {operation}");

        var placeholderError = ValidatePlaceholders(operation, request.Content);
        if (placeholderError != null)
            return ApiResponse<AiPromptResponse>.FailureResponse(placeholderError);

        var history = new AiPromptHistory
        {
            AiPromptId = entity.Id,
            VersionNumber = entity.VersionNumber,
            Provider = entity.Provider,
            Content = entity.Content,
            Note = request.Note,
        };

        entity.Content = request.Content;
        entity.Provider = (AiProvider)request.Provider;
        entity.VersionNumber += 1;

        if (request.Name != null)
            entity.Name = request.Name;
        if (request.Description != null)
            entity.Description = request.Description;

        await _unitOfWork.AiPromptHistories.AddAsync(history);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync(CacheKey(operation));

        _logger.LogInformation("Updated prompt {Operation} to version {Version}", operation, entity.VersionNumber);

        return ApiResponse<AiPromptResponse>.SuccessResponse(entity.ToResponse(), "Cập nhật prompt thành công");
    }

    public async Task<ApiResponse<List<AiPromptVersionResponse>>> GetVersionsAsync(
        AiOperation operation,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.AiPrompts.GetByOperationAsync(operation, cancellationToken);
        if (entity == null)
            return ApiResponse<List<AiPromptVersionResponse>>.NotFoundResponse($"Không tìm thấy prompt cho operation {operation}");

        var histories = await _unitOfWork.AiPromptHistories.GetByPromptIdAsync(entity.Id, cancellationToken);
        var responses = histories.Select(h => h.ToVersionResponse(entity.VersionNumber)).ToList();

        return ApiResponse<List<AiPromptVersionResponse>>.SuccessResponse(responses);
    }

    public async Task<ApiResponse<AiPromptResponse>> RollbackAsync(
        AiOperation operation,
        RollbackAiPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.AiPrompts.GetByOperationAsync(operation, cancellationToken);
        if (entity == null)
            return ApiResponse<AiPromptResponse>.NotFoundResponse($"Không tìm thấy prompt cho operation {operation}");

        if (request.VersionNumber == entity.VersionNumber)
            return ApiResponse<AiPromptResponse>.FailureResponse($"Version {request.VersionNumber} đang là version hiện tại, không thể rollback");

        var historyRecord = await _unitOfWork.AiPromptHistories.GetByVersionAsync(entity.Id, request.VersionNumber, cancellationToken);
        if (historyRecord == null)
            return ApiResponse<AiPromptResponse>.NotFoundResponse($"Không tìm thấy version {request.VersionNumber}");

        var snapshotHistory = new AiPromptHistory
        {
            AiPromptId = entity.Id,
            VersionNumber = entity.VersionNumber,
            Provider = entity.Provider,
            Content = entity.Content,
            Note = request.Note ?? $"Rollback từ version {entity.VersionNumber} về version {request.VersionNumber}",
        };

        entity.Content = historyRecord.Content;
        entity.Provider = historyRecord.Provider;
        entity.VersionNumber += 1;

        await _unitOfWork.AiPromptHistories.AddAsync(snapshotHistory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync(CacheKey(operation));

        _logger.LogInformation("Rolled back prompt {Operation} to content from version {SourceVersion}, new version {NewVersion}",
            operation, request.VersionNumber, entity.VersionNumber);

        return ApiResponse<AiPromptResponse>.SuccessResponse(entity.ToResponse(), "Rollback prompt thành công");
    }

    private static string? ValidatePlaceholders(AiOperation operation, string content)
    {
        if (!RequiredPlaceholders.TryGetValue(operation, out var placeholders))
            return null;

        var missing = placeholders.Where(p => !content.Contains(p, StringComparison.Ordinal)).ToList();
        if (missing.Count > 0)
            return $"Content thiếu placeholder bắt buộc: {string.Join(", ", missing)}";

        return null;
    }
}
