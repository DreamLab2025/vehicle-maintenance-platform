using Moq;
using Verendar.Common.Databases.Interfaces;
using Verendar.Identity.Domain.Entities;
using Verendar.Identity.Domain.Repositories.Interfaces;

namespace Verendar.Identity.Tests.Support;

internal sealed class IdentityUnitOfWorkMock
{
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Loose);
    public Mock<IUserRepository> Users { get; } = new(MockBehavior.Strict);
    public Mock<IGenericRepository<Feedback>> Feedbacks { get; } = new(MockBehavior.Strict);

    public IdentityUnitOfWorkMock()
    {
        UnitOfWork.Setup(u => u.Users).Returns(Users.Object);
        UnitOfWork.Setup(u => u.Feedbacks).Returns(Feedbacks.Object);
        UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }
}
