using Microsoft.Extensions.Options;
using Verendar.Ai.Application.Options;

namespace Verendar.Ai.Application.Jobs;

public class AiPromptRetentionJob(
    IUnitOfWork unitOfWork,
    IOptions<PromptVersioningOptions> options,
    ILogger<AiPromptRetentionJob> logger)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly int _maxVersions = options.Value.MaxVersionsPerPrompt;
    private readonly ILogger<AiPromptRetentionJob> _logger = logger;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var prompts = await _unitOfWork.AiPrompts.GetAllPromptsAsync(cancellationToken);

        foreach (var prompt in prompts)
        {
            var historyCount = await _unitOfWork.AiPromptHistories.CountAsync(
                h => h.AiPromptId == prompt.Id);

            if (historyCount <= _maxVersions)
                continue;

            var removed = await _unitOfWork.AiPromptHistories.RemoveExcessVersionsAsync(
                prompt.Id,
                _maxVersions,
                cancellationToken);

            _logger.LogInformation(
                "Retention: removed {Count} old versions for prompt {Operation} (kept top {Max})",
                removed, prompt.Operation, _maxVersions);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
