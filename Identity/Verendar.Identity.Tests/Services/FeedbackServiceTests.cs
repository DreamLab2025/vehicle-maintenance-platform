using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Common.Shared;
using Verendar.Identity.Application.Dtos;
using Verendar.Identity.Application.Services.Implements;
using Verendar.Identity.Domain.Entities;
using Verendar.Identity.Domain.Enums;

namespace Verendar.Identity.Tests.Services;

public class FeedbackServiceTests
{
    // ──────────────────────────────────────────────
    // SubmitAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task SubmitAsync_WhenValidRequest_CreatesFeedbackAndReturns201()
    {
        var userId = Guid.NewGuid();
        var request = new CreateFeedbackRequest
        {
            Category = FeedbackCategory.Bug,
            Subject = "Lỗi đăng nhập",
            Content = "Không thể đăng nhập bằng Google trên iOS.",
            Rating = 3,
            ContactEmail = "user@example.com"
        };

        var m = new IdentityUnitOfWorkMock();
        m.Feedbacks.Setup(r => r.AddAsync(It.IsAny<Feedback>()))
            .ReturnsAsync((Feedback f) => f);

        var sut = new FeedbackService(NullLogger<FeedbackService>.Instance, m.UnitOfWork.Object);
        var result = await sut.SubmitAsync(userId, request);

        IdentityServiceResponseAssert.AssertCreated(result, "Gửi feedback thành công.");
        result.Data.Should().NotBeNull();
        result.Data!.UserId.Should().Be(userId);
        result.Data.Category.Should().Be(FeedbackCategory.Bug);
        result.Data.Subject.Should().Be(request.Subject);
        result.Data.Content.Should().Be(request.Content);
        result.Data.Rating.Should().Be(3);
        result.Data.ContactEmail.Should().Be("user@example.com");
        result.Data.Status.Should().Be(FeedbackStatus.Pending);
        m.Feedbacks.Verify(r => r.AddAsync(It.IsAny<Feedback>()), Times.Once);
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAsync_WhenNoOptionalFields_CreatesFeedbackWithNullRatingAndEmail()
    {
        var userId = Guid.NewGuid();
        var request = new CreateFeedbackRequest
        {
            Category = FeedbackCategory.General,
            Subject = "Góp ý chung",
            Content = "Ứng dụng hoạt động ổn định, mong có thêm tính năng mới."
        };

        var m = new IdentityUnitOfWorkMock();
        m.Feedbacks.Setup(r => r.AddAsync(It.IsAny<Feedback>()))
            .ReturnsAsync((Feedback f) => f);

        var sut = new FeedbackService(NullLogger<FeedbackService>.Instance, m.UnitOfWork.Object);
        var result = await sut.SubmitAsync(userId, request);

        IdentityServiceResponseAssert.AssertCreated(result, "Gửi feedback thành công.");
        result.Data!.Rating.Should().BeNull();
        result.Data.ContactEmail.Should().BeNull();
    }

    // ──────────────────────────────────────────────
    // GetAllAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_WhenHasData_ReturnsPagedFeedbacks()
    {
        var feedbacks = new List<Feedback>
        {
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Category = FeedbackCategory.Bug,     Subject = "Bug 1", Content = "Mô tả lỗi 1", Status = FeedbackStatus.Pending },
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Category = FeedbackCategory.Feature, Subject = "Đề xuất", Content = "Thêm tính năng X", Status = FeedbackStatus.Reviewed }
        };

        var m = new IdentityUnitOfWorkMock();
        m.Feedbacks.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>()))
            .ReturnsAsync((feedbacks, feedbacks.Count));

        var sut = new FeedbackService(NullLogger<FeedbackService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetAllAsync(new PaginationRequest { PageNumber = 1, PageSize = 10 });

        var meta = IdentityServiceResponseAssert.AssertPagedSuccess(result, "Lấy danh sách feedback thành công.", 2, 2);
        meta.PageNumber.Should().Be(1);
        meta.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ReturnsEmptyList()
    {
        var m = new IdentityUnitOfWorkMock();
        m.Feedbacks.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>()))
            .ReturnsAsync((new List<Feedback>(), 0));

        var sut = new FeedbackService(NullLogger<FeedbackService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetAllAsync(new PaginationRequest { PageNumber = 1, PageSize = 10 });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
        var meta = result.Metadata.Should().BeOfType<PagingMetadata>().Subject;
        meta.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task GetAllAsync_WhenPageNumberIsZero_NormalizesToPageOne()
    {
        var m = new IdentityUnitOfWorkMock();
        m.Feedbacks.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>()))
            .ReturnsAsync((new List<Feedback>(), 0));

        var sut = new FeedbackService(NullLogger<FeedbackService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetAllAsync(new PaginationRequest { PageNumber = 0, PageSize = 0 });

        result.IsSuccess.Should().BeTrue();
        var meta = result.Metadata.Should().BeOfType<PagingMetadata>().Subject;
        meta.PageNumber.Should().Be(1);
        meta.PageSize.Should().Be(10);
    }

    // ──────────────────────────────────────────────
    // GetByIdAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsFeedback()
    {
        var feedbackId = Guid.NewGuid();
        var feedback = new Feedback
        {
            Id = feedbackId,
            UserId = Guid.NewGuid(),
            Category = FeedbackCategory.UX,
            Subject = "UX issue",
            Content = "Nút đăng xuất khó tìm.",
            Status = FeedbackStatus.Pending
        };

        var m = new IdentityUnitOfWorkMock();
        m.Feedbacks.Setup(r => r.GetByIdAsync(feedbackId))
            .ReturnsAsync(feedback);

        var sut = new FeedbackService(NullLogger<FeedbackService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetByIdAsync(feedbackId);

        IdentityServiceResponseAssert.AssertSuccess(result, "Lấy feedback thành công.");
        result.Data!.Id.Should().Be(feedbackId);
        result.Data.Category.Should().Be(FeedbackCategory.UX);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_Returns404()
    {
        var m = new IdentityUnitOfWorkMock();
        m.Feedbacks.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Feedback?)null);

        var sut = new FeedbackService(NullLogger<FeedbackService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetByIdAsync(Guid.NewGuid());

        IdentityServiceResponseAssert.AssertFailure(result, 404, "Feedback không tồn tại.");
    }

    // ──────────────────────────────────────────────
    // UpdateStatusAsync
    // ──────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatusAsync_WhenNotFound_Returns404()
    {
        var m = new IdentityUnitOfWorkMock();
        m.Feedbacks.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Feedback?)null);

        var sut = new FeedbackService(NullLogger<FeedbackService>.Instance, m.UnitOfWork.Object);
        var result = await sut.UpdateStatusAsync(Guid.NewGuid(), new UpdateFeedbackStatusRequest { Status = FeedbackStatus.Reviewed });

        IdentityServiceResponseAssert.AssertFailure(result, 404, "Feedback không tồn tại.");
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenResolvedToRevertPending_Returns400()
    {
        var feedbackId = Guid.NewGuid();
        var feedback = new Feedback
        {
            Id = feedbackId,
            UserId = Guid.NewGuid(),
            Category = FeedbackCategory.Bug,
            Subject = "Lỗi",
            Content = "Mô tả lỗi chi tiết.",
            Status = FeedbackStatus.Resolved
        };

        var m = new IdentityUnitOfWorkMock();
        m.Feedbacks.Setup(r => r.GetByIdAsync(feedbackId))
            .ReturnsAsync(feedback);

        var sut = new FeedbackService(NullLogger<FeedbackService>.Instance, m.UnitOfWork.Object);
        var result = await sut.UpdateStatusAsync(feedbackId, new UpdateFeedbackStatusRequest { Status = FeedbackStatus.Pending });

        IdentityServiceResponseAssert.AssertFailure(result, 400, "Không thể đặt lại trạng thái Pending sau khi đã Resolved.");
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(FeedbackStatus.Pending, FeedbackStatus.Reviewed)]
    [InlineData(FeedbackStatus.Pending, FeedbackStatus.Resolved)]
    [InlineData(FeedbackStatus.Reviewed, FeedbackStatus.Resolved)]
    public async Task UpdateStatusAsync_WhenValidTransition_UpdatesStatusAndReturns200(
        FeedbackStatus currentStatus, FeedbackStatus newStatus)
    {
        var feedbackId = Guid.NewGuid();
        var feedback = new Feedback
        {
            Id = feedbackId,
            UserId = Guid.NewGuid(),
            Category = FeedbackCategory.Performance,
            Subject = "Chậm",
            Content = "Ứng dụng phản hồi chậm lúc giờ cao điểm.",
            Status = currentStatus
        };

        var m = new IdentityUnitOfWorkMock();
        m.Feedbacks.Setup(r => r.GetByIdAsync(feedbackId))
            .ReturnsAsync(feedback);
        m.Feedbacks.Setup(r => r.UpdateAsync(feedbackId, feedback))
            .Returns(Task.CompletedTask);

        var sut = new FeedbackService(NullLogger<FeedbackService>.Instance, m.UnitOfWork.Object);
        var result = await sut.UpdateStatusAsync(feedbackId, new UpdateFeedbackStatusRequest { Status = newStatus });

        IdentityServiceResponseAssert.AssertSuccess(result, "Cập nhật trạng thái feedback thành công.");
        result.Data!.Status.Should().Be(newStatus);
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
