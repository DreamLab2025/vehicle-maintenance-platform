using Microsoft.Extensions.Options;
using Verendar.Ai.Infrastructure.Configuration;
using Verendar.Ai.Infrastructure.Data;

namespace Verendar.Ai.Infrastructure.Jobs;

public class AiPromptRetentionJob(
    AiDbContext context,
    IOptions<PromptVersioningOptions> options,
    ILogger<AiPromptRetentionJob> logger)
{
    private readonly AiDbContext _context = context;
    private readonly int _maxVersions = options.Value.MaxVersionsPerPrompt;
    private readonly ILogger<AiPromptRetentionJob> _logger = logger;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var prompts = await _context.AiPrompts
            .Where(p => p.DeletedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var prompt in prompts)
        {
            var historyCount = await _context.AiPromptHistories
                .CountAsync(h => h.AiPromptId == prompt.Id, cancellationToken);

            if (historyCount <= _maxVersions)
                continue;

            var expiredRecords = await _context.AiPromptHistories
                .Where(h => h.AiPromptId == prompt.Id)
                .OrderByDescending(h => h.VersionNumber)
                .Skip(_maxVersions)
                .ToListAsync(cancellationToken);

            _context.AiPromptHistories.RemoveRange(expiredRecords);

            _logger.LogInformation(
                "Retention: removed {Count} old versions for prompt {Operation} (kept top {Max})",
                expiredRecords.Count, prompt.Operation, _maxVersions);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
