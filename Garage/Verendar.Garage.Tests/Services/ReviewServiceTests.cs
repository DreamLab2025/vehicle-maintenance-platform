using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Common.Shared;
using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Implements;
using Verendar.Garage.Application.Services.Interfaces;
using Verendar.Garage.Domain.Entities;
using Verendar.Garage.Domain.Enums;

namespace Verendar.Garage.Tests.Services;

public class ReviewServiceTests
{
    [Fact]
    public async Task SubmitReviewAsync_WhenBookingNotFound_Returns404()
    {
        var m = new GarageUnitOfWorkMock();
        m.Bookings.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Booking, bool>>>()))
            .ReturnsAsync((Booking?)null);

        var sut = new ReviewService(NullLogger<ReviewService>.Instance, m.UnitOfWork.Object, Mock.Of<IBookingService>());
        var result = await sut.SubmitReviewAsync(Guid.NewGuid(), Guid.NewGuid(), new CreateReviewRequest { Rating = 5 });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, "Không tìm thấy lịch hẹn này.");
        m.Reviews.Verify(r => r.AddAsync(It.IsAny<GarageReview>()), Times.Never);
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitReviewAsync_WhenBookingBelongsToAnotherUser_Returns403()
    {
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GarageBranchId = Guid.NewGuid(),
            Status = BookingStatus.Completed
        };

        var m = new GarageUnitOfWorkMock();
        m.Bookings.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Booking, bool>>>()))
            .ReturnsAsync(booking);

        var sut = new ReviewService(NullLogger<ReviewService>.Instance, m.UnitOfWork.Object, Mock.Of<IBookingService>());
        var result = await sut.SubmitReviewAsync(Guid.NewGuid(), booking.Id, new CreateReviewRequest { Rating = 4 });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 403, EndpointMessages.Review.ReviewForbidden);
    }

    [Fact]
    public async Task SubmitReviewAsync_WhenBookingNotCompleted_Returns400()
    {
        var userId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GarageBranchId = Guid.NewGuid(),
            Status = BookingStatus.Pending
        };

        var m = new GarageUnitOfWorkMock();
        m.Bookings.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Booking, bool>>>()))
            .ReturnsAsync(booking);

        var sut = new ReviewService(NullLogger<ReviewService>.Instance, m.UnitOfWork.Object, Mock.Of<IBookingService>());
        var result = await sut.SubmitReviewAsync(userId, booking.Id, new CreateReviewRequest { Rating = 4 });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 400, EndpointMessages.Review.BookingNotCompleted);
    }

    [Fact]
    public async Task SubmitReviewAsync_WhenReviewAlreadyExists_Returns409()
    {
        var userId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GarageBranchId = Guid.NewGuid(),
            Status = BookingStatus.Completed
        };

        var m = new GarageUnitOfWorkMock();
        m.Bookings.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Booking, bool>>>()))
            .ReturnsAsync(booking);
        m.Reviews.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageReview, bool>>>()))
            .ReturnsAsync(new GarageReview { Id = Guid.NewGuid(), BookingId = booking.Id });

        var sut = new ReviewService(NullLogger<ReviewService>.Instance, m.UnitOfWork.Object, Mock.Of<IBookingService>());
        var result = await sut.SubmitReviewAsync(userId, booking.Id, new CreateReviewRequest { Rating = 4 });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 409, "Lịch hẹn này đã được đánh giá.");
        m.Reviews.Verify(r => r.AddAsync(It.IsAny<GarageReview>()), Times.Never);
    }

    [Fact]
    public async Task SubmitReviewAsync_WhenValidRequest_CreatesReviewAndReturns201()
    {
        var userId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GarageBranchId = Guid.NewGuid(),
            Status = BookingStatus.Completed
        };

        var m = new GarageUnitOfWorkMock();
        m.Bookings.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Booking, bool>>>()))
            .ReturnsAsync(booking);
        m.Reviews.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageReview, bool>>>()))
            .ReturnsAsync((GarageReview?)null);
        m.Reviews.Setup(r => r.AddAsync(It.IsAny<GarageReview>()))
            .ReturnsAsync((GarageReview review) => review);

        var sut = new ReviewService(NullLogger<ReviewService>.Instance, m.UnitOfWork.Object, Mock.Of<IBookingService>());
        var result = await sut.SubmitReviewAsync(userId, booking.Id, new CreateReviewRequest { Rating = 5, Comment = "Rất tốt" });

        GarageServiceResponseAssert.AssertCreatedEnvelope(result, EndpointMessages.Review.SubmitSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.BookingId.Should().Be(booking.Id);
        result.Data.Rating.Should().Be(5);
        m.Reviews.Verify(r => r.AddAsync(It.IsAny<GarageReview>()), Times.Once);
        m.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByBookingAsync_WhenNotFound_Returns404()
    {
        var m = new GarageUnitOfWorkMock();
        m.Reviews.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageReview, bool>>>()))
            .ReturnsAsync((GarageReview?)null);

        var sut = new ReviewService(NullLogger<ReviewService>.Instance, m.UnitOfWork.Object, Mock.Of<IBookingService>());
        var result = await sut.GetByBookingAsync(Guid.NewGuid(), Guid.NewGuid());

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, EndpointMessages.Review.ReviewNotFound);
    }

    [Fact]
    public async Task GetByBranchAsync_WhenHasData_ReturnsPagedReviews()
    {
        var branchId = Guid.NewGuid();
        var reviews = new List<GarageReview>
        {
            new() { Id = Guid.NewGuid(), GarageBranchId = branchId, BookingId = Guid.NewGuid(), UserId = Guid.NewGuid(), Rating = 5 },
            new() { Id = Guid.NewGuid(), GarageBranchId = branchId, BookingId = Guid.NewGuid(), UserId = Guid.NewGuid(), Rating = 4 }
        };

        var m = new GarageUnitOfWorkMock();
        m.Reviews.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<GarageReview, bool>>>(),
                It.IsAny<Func<IQueryable<GarageReview>, IOrderedQueryable<GarageReview>>>()))
            .ReturnsAsync((reviews, reviews.Count));

        var sut = new ReviewService(NullLogger<ReviewService>.Instance, m.UnitOfWork.Object, Mock.Of<IBookingService>());
        var result = await sut.GetByBranchAsync(branchId, new PaginationRequest { PageNumber = 0, PageSize = 0 });

        var metadata = GarageServiceResponseAssert.AssertPagedSuccessEnvelope(result, EndpointMessages.Review.GetBranchReviewsSuccess, 2, 2);
        metadata.PageNumber.Should().Be(1);
        metadata.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(2);
    }
}
