using System.Linq.Expressions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Common.Shared;
using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Clients;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Implements;
using Verendar.Garage.Domain.Entities;
using Verendar.Garage.Domain.Enums;
using GarageEntity = Verendar.Garage.Domain.Entities.Garage;

namespace Verendar.Garage.Tests.Services;

public class BookingServiceTests
{
    [Fact]
    public async Task CreateBookingAsync_WhenBranchNotFound_Returns404()
    {
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync((GarageBranch?)null);

        var sut = CreateSut(m);
        var result = await sut.CreateBookingAsync(Guid.NewGuid(), new CreateBookingRequest { GarageBranchId = Guid.NewGuid() });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, "Không tìm thấy chi nhánh. Bạn vui lòng kiểm tra lại hoặc chọn chi nhánh khác.");
    }

    [Fact]
    public async Task CreateBookingAsync_WhenGarageNotFound_Returns404()
    {
        var branch = new GarageBranch { Id = Guid.NewGuid(), GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync((GarageEntity?)null);

        var sut = CreateSut(m);
        var result = await sut.CreateBookingAsync(Guid.NewGuid(), new CreateBookingRequest { GarageBranchId = branch.Id });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, EndpointMessages.Booking.GarageNotFound);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenGarageInactive_Returns400()
    {
        var branch = new GarageBranch { Id = Guid.NewGuid(), GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new(), Status = BranchStatus.Active };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = Guid.NewGuid(), BusinessName = "G", Slug = "g", Status = GarageStatus.Pending };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);

        var sut = CreateSut(m);
        var result = await sut.CreateBookingAsync(Guid.NewGuid(), new CreateBookingRequest { GarageBranchId = branch.Id });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 400, EndpointMessages.Booking.GarageInactive);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenBranchInactive_Returns400()
    {
        var branch = new GarageBranch { Id = Guid.NewGuid(), GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new(), Status = BranchStatus.Inactive };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = Guid.NewGuid(), BusinessName = "G", Slug = "g", Status = GarageStatus.Active };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);

        var sut = CreateSut(m);
        var result = await sut.CreateBookingAsync(Guid.NewGuid(), new CreateBookingRequest { GarageBranchId = branch.Id });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 400, EndpointMessages.Booking.BranchInactive);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenNoItems_Returns422()
    {
        var branch = new GarageBranch { Id = Guid.NewGuid(), GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new(), Status = BranchStatus.Active };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = Guid.NewGuid(), BusinessName = "G", Slug = "g", Status = GarageStatus.Active };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);

        var sut = CreateSut(m);
        var result = await sut.CreateBookingAsync(Guid.NewGuid(), new CreateBookingRequest
        {
            GarageBranchId = branch.Id,
            ScheduledAt = DateTime.UtcNow.AddHours(2)
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 422, EndpointMessages.Booking.EmptyItems);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenScheduledInPast_Returns400()
    {
        var branch = new GarageBranch { Id = Guid.NewGuid(), GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new(), Status = BranchStatus.Active };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = Guid.NewGuid(), BusinessName = "G", Slug = "g", Status = GarageStatus.Active };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);

        var sut = CreateSut(m);
        var result = await sut.CreateBookingAsync(Guid.NewGuid(), new CreateBookingRequest
        {
            GarageBranchId = branch.Id,
            ScheduledAt = DateTime.UtcNow.AddMinutes(-5),
            Items = [new CreateBookingLineItemRequest { ProductId = Guid.NewGuid(), ServiceId = Guid.NewGuid() }]
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 400, EndpointMessages.Booking.ScheduleMustBeFuture);
    }

    [Fact]
    public async Task GetBookingsAsync_WhenAssignedToMeCombinedWithFilters_Returns400()
    {
        var m = new GarageUnitOfWorkMock();
        var sut = CreateSut(m);

        var result = await sut.GetBookingsAsync(
            Guid.NewGuid(),
            assignedToMe: true,
            branchId: Guid.NewGuid(),
            userId: null,
            pagination: new PaginationRequest());

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 400, EndpointMessages.Booking.AssignedToMeConflict);
    }

    [Fact]
    public async Task GetBookingsAsync_WhenUserIdAndCurrentUserMismatch_Returns403()
    {
        var m = new GarageUnitOfWorkMock();
        var sut = CreateSut(m);

        var result = await sut.GetBookingsAsync(
            currentUserId: Guid.NewGuid(),
            assignedToMe: false,
            branchId: null,
            userId: Guid.NewGuid(),
            pagination: new PaginationRequest());

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 403, EndpointMessages.Booking.UserMismatchForbidden);
    }

    [Fact]
    public async Task GetBookingsAsync_WhenAssignedToUserWithoutMechanicRole_Returns403()
    {
        var m = new GarageUnitOfWorkMock();
        m.Members.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<GarageMember, bool>>>()))
            .ReturnsAsync([]);

        var sut = CreateSut(m);
        var result = await sut.GetBookingsAsync(
            currentUserId: Guid.NewGuid(),
            assignedToMe: true,
            branchId: null,
            userId: null,
            pagination: new PaginationRequest());

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 403, EndpointMessages.Booking.NotMechanicForbidden);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenBookingNotFound_Returns404()
    {
        var m = new GarageUnitOfWorkMock();
        m.Bookings.Setup(r => r.GetByIdTrackedWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        var sut = CreateSut(m);
        var result = await sut.CancelBookingAsync(Guid.NewGuid(), Guid.NewGuid(), "Không rảnh");

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be(EndpointMessages.Booking.BookingNotFound);
    }

    private static BookingService CreateSut(GarageUnitOfWorkMock mock)
    {
        return new BookingService(
            NullLogger<BookingService>.Instance,
            mock.UnitOfWork.Object,
            Mock.Of<IPublishEndpoint>(),
            Mock.Of<IGarageIdentityContactClient>(),
            Mock.Of<IVehicleGarageClient>());
    }
}
