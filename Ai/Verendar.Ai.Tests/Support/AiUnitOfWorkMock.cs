namespace Verendar.Ai.Tests.Support;

using Moq;
using Verendar.Ai.Domain.Repositories.Interfaces;

internal sealed class AiUnitOfWorkMock
{
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Loose);
    public Mock<IAiUsageRepository> AiUsages { get; } = new(MockBehavior.Strict);

    public AiUnitOfWorkMock()
    {
        UnitOfWork.Setup(u => u.AiUsages).Returns(AiUsages.Object);
    }
}
