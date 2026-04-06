using System.Linq.Expressions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Identity.Application.Dtos;
using Verendar.Identity.Application.Services.Implements;
using Verendar.Identity.Domain.Entities;
using Verender.Identity.Contracts.Events;

namespace Verendar.Identity.Tests.Services;

public class UserServiceTests
{
    private static UserService CreateSut(IdentityUnitOfWorkMock m, Mock<IPublishEndpoint>? publish = null)
    {
        publish ??= new Mock<IPublishEndpoint>(MockBehavior.Loose);
        publish.Setup(p => p.Publish(It.IsAny<ForceTokenRefreshEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new UserService(m.UnitOfWork.Object, publish.Object, NullLogger<UserService>.Instance);
    }

    [Fact]
    public async Task CreateUserAsync_WhenEmailExists_Returns409()
    {
        var existing = new User { Id = Guid.NewGuid(), Email = "a@b.com", FullName = "X", PasswordHash = "h", Roles = [UserRole.User] };
        var m = new IdentityUnitOfWorkMock();
        m.Users.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(existing);
        m.Users.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var request = new UserCreateRequest
        {
            FullName = "New User",
            Email = "a@b.com",
            Password = "Abcd1234",
            Roles = [UserRole.User]
        };

        var sut = CreateSut(m);
        var result = await sut.CreateUserAsync(request);

        IdentityServiceResponseAssert.AssertFailure(result, 409, "Email đã được đăng ký.");
        m.Users.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_WhenValid_Returns201AndPersists()
    {
        var m = new IdentityUnitOfWorkMock();
        m.Users.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync((User?)null);
        m.Users.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var request = new UserCreateRequest
        {
            FullName = "Admin Created",
            Email = "new@example.com",
            Password = "Abcd1234",
            PhoneNumber = "0901234567",
            Roles = [UserRole.User, UserRole.GarageOwner]
        };

        var sut = CreateSut(m);
        var result = await sut.CreateUserAsync(request);

        IdentityServiceResponseAssert.AssertCreated(result, "Tạo người dùng thành công.");
        result.Data!.Email.Should().Be("new@example.com");
        result.Data.Roles.Should().Contain("User").And.Contain("GarageOwner");
        m.Users.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.FullName == "Admin Created"
            && u.Email == "new@example.com"
            && u.Roles.Count == 2)), Times.Once);
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        var m = new IdentityUnitOfWorkMock();
        m.Users.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User?)null);

        var request = new UserUpdateRequest
        {
            FullName = "X",
            Email = "x@y.com",
            Roles = [UserRole.User]
        };

        var sut = CreateSut(m);
        var result = await sut.UpdateUserAsync(id, request);

        IdentityServiceResponseAssert.AssertFailure(result, 404, "Người dùng không tồn tại.");
    }

    [Fact]
    public async Task UpdateUserAsync_WhenEmailTakenByOther_Returns409()
    {
        var id = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var user = new User
        {
            Id = id,
            Email = "old@example.com",
            FullName = "Old",
            PasswordHash = "hash",
            Roles = [UserRole.User]
        };
        var other = new User { Id = otherId, Email = "taken@example.com", FullName = "Other", PasswordHash = "h", Roles = [UserRole.User] };

        var m = new IdentityUnitOfWorkMock();
        m.Users.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);
        m.Users.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(other);

        var request = new UserUpdateRequest
        {
            FullName = "Old",
            Email = "taken@example.com",
            Roles = [UserRole.User]
        };

        var sut = CreateSut(m);
        var result = await sut.UpdateUserAsync(id, request);

        IdentityServiceResponseAssert.AssertFailure(result, 409, "Email đã được sử dụng bởi tài khoản khác.");
    }

    [Fact]
    public async Task UpdateUserAsync_WhenValid_UpdatesAndReturns200()
    {
        var id = Guid.NewGuid();
        var user = new User
        {
            Id = id,
            Email = "same@example.com",
            FullName = "Before",
            PasswordHash = "hash",
            Roles = [UserRole.User],
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var m = new IdentityUnitOfWorkMock();
        m.Users.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);
        m.Users.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync((User?)null);

        var request = new UserUpdateRequest
        {
            FullName = "After",
            Email = "same@example.com",
            EmailVerified = true,
            Roles = [UserRole.Admin]
        };

        var sut = CreateSut(m);
        var result = await sut.UpdateUserAsync(id, request);

        IdentityServiceResponseAssert.AssertSuccess(result, "Cập nhật người dùng thành công.");
        result.Data!.UserName.Should().Be("After");
        result.Data.Roles.Should().Contain("Admin");
        user.FullName.Should().Be("After");
        user.EmailVerified.Should().BeTrue();
        user.Roles.Should().Equal(UserRole.Admin);
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenSelf_Returns400()
    {
        var id = Guid.NewGuid();
        var m = new IdentityUnitOfWorkMock();
        var sut = CreateSut(m);

        var result = await sut.DeleteUserAsync(id, id);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be("Không thể xóa tài khoản của chính bạn.");
        result.Data.Should().BeFalse();
        m.Users.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        var actor = Guid.NewGuid();
        var m = new IdentityUnitOfWorkMock();
        m.Users.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User?)null);

        var sut = CreateSut(m);
        var result = await sut.DeleteUserAsync(id, actor);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("Người dùng không tồn tại.");
        result.Data.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserAsync_WhenValid_SoftDeletes()
    {
        var id = Guid.NewGuid();
        var actor = Guid.NewGuid();
        var user = new User
        {
            Id = id,
            Email = "del@example.com",
            FullName = "Del",
            PasswordHash = "h",
            Roles = [UserRole.User],
            RefreshToken = "rt",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        var m = new IdentityUnitOfWorkMock();
        m.Users.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);

        var sut = CreateSut(m);
        var result = await sut.DeleteUserAsync(id, actor);

        IdentityServiceResponseAssert.AssertSuccess(result, "Xóa người dùng thành công.");
        result.Data.Should().BeTrue();
        user.DeletedAt.Should().NotBeNull();
        user.RefreshToken.Should().BeEmpty();
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
