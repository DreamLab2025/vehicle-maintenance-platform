using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Common.Databases.UnitOfWork;


namespace Verendar.Ai.Infrastructure.Repositories.Implements
{
    public class UnitOfWork(AiDbContext context) : BaseUnitOfWork<AiDbContext>(context), IUnitOfWork
    {
        private IAiUsageRepository? _aiUsages;
        private IAiPromptRepository? _aiPrompts;
        private IAiPromptHistoryRepository? _aiPromptHistories;

        public IAiUsageRepository AiUsages => _aiUsages ??= new AiUsageRepository(Context);
        public IAiPromptRepository AiPrompts => _aiPrompts ??= new AiPromptRepository(Context);
        public IAiPromptHistoryRepository AiPromptHistories => _aiPromptHistories ??= new AiPromptHistoryRepository(Context);
    }
}
