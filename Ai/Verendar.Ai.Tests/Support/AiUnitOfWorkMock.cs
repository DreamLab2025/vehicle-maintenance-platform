namespace Verendar.Ai.Tests.Support;

using Moq;
using Verendar.Ai.Domain.Repositories.Interfaces;

internal sealed class AiUnitOfWorkMock
{
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Loose);
    public Mock<IAiUsageRepository> AiUsages { get; } = new(MockBehavior.Strict);
    public Mock<IAiPromptRepository> AiPrompts { get; } = new(MockBehavior.Strict);
    public Mock<IAiPromptHistoryRepository> AiPromptHistories { get; } = new(MockBehavior.Strict);

    public AiUnitOfWorkMock()
    {
        UnitOfWork.Setup(u => u.AiUsages).Returns(AiUsages.Object);
        UnitOfWork.Setup(u => u.AiPrompts).Returns(AiPrompts.Object);
        UnitOfWork.Setup(u => u.AiPromptHistories).Returns(AiPromptHistories.Object);
    }
}
