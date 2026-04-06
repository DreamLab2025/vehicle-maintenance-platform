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
using Verendar.Garage.Domain.Models;
using Verendar.Garage.Domain.ValueObjects;
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
            UserVehicleId = Guid.NewGuid(),
            ScheduledAt = DateTime.UtcNow.AddHours(2)
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 422, EndpointMessages.Booking.EmptyItems);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenScheduledInPast_Returns400()
    {
        var branch = new GarageBranch { Id = Guid.NewGuid(), GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new(), Status = BranchStatus.Active };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = Guid.NewGuid(), BusinessName = "G", Slug = "g", Status = GarageStatus.Active };
        var productId = Guid.NewGuid();
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.GarageProducts.Setup(r => r.GetByIdWithInstallationAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GarageProduct
            {
                Id = productId,
                GarageBranchId = branch.Id,
                Name = "Oil",
                Status = ProductStatus.Active,
                MaterialPrice = new Money { Amount = 100, Currency = "VND" }
            });

        var sut = CreateSut(m);
        var result = await sut.CreateBookingAsync(Guid.NewGuid(), new CreateBookingRequest
        {
            GarageBranchId = branch.Id,
            UserVehicleId = Guid.NewGuid(),
            ScheduledAt = DateTime.UtcNow.AddMinutes(-5),
            Items = [new CreateBookingLineItemRequest { ProductId = productId }]
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
            status: null,
            pagination: new PaginationRequest());

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 400, EndpointMessages.Booking.AssignedToMeConflict);
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
            status: null,
            pagination: new PaginationRequest());

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 403, EndpointMessages.Booking.NotMechanicForbidden);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenBookingNotFound_Returns404()
    {
        var m = new GarageUnitOfWorkMock();
        m.Bookings.Setup(r => r.GetByIdTrackedForMutationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        var sut = CreateSut(m);
        var result = await sut.CancelBookingAsync(Guid.NewGuid(), Guid.NewGuid(), "Không rảnh");

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be(EndpointMessages.Booking.BookingNotFound);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenActorNotCustomerAndBranchMissing_Returns404()
    {
        var bookingId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var actorId = Guid.NewGuid();

        var booking = new Booking
        {
            Id = bookingId,
            GarageBranchId = branchId,
            UserId = customerId,
            Status = BookingStatus.Pending,
            BookedTotalPrice = new Money { Amount = 0, Currency = "VND" }
        };

        var m = new GarageUnitOfWorkMock();
        m.Bookings.Setup(r => r.GetByIdTrackedForMutationAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        m.GarageBranches.Setup(r => r.GetGarageOwnerIdByBranchIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var sut = CreateSut(m);
        var result = await sut.CancelBookingAsync(bookingId, actorId, null);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be(EndpointMessages.Booking.BranchNotFound);
    }

    [Fact]
    public async Task AssignMechanicAsync_WhenTryAssignReturnsFalse_Returns409()
    {
        var bookingId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var mechanicMemberId = Guid.NewGuid();

        var m = new GarageUnitOfWorkMock();
        m.Bookings.Setup(r => r.GetAssignmentSnapshotAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookingAssignmentSnapshot(bookingId, BookingStatus.Pending, branchId, userId, DateTime.UtcNow));
        m.GarageBranches.Setup(r => r.GetGarageOwnerIdByBranchIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerId);
        m.Members.Setup(r => r.GetActiveMechanicForAssignmentAsync(mechanicMemberId, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((mechanicMemberId, "Thợ A"));
        m.Bookings.Setup(r => r.TryAssignMechanicPersistAsync(
                bookingId,
                mechanicMemberId,
                BookingStatus.Pending,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = CreateSut(m);
        var result = await sut.AssignMechanicAsync(bookingId, ownerId, new AssignBookingRequest { GarageMemberId = mechanicMemberId });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 409, EndpointMessages.Booking.AssignConcurrentlyModified);
        m.Bookings.Verify(
            r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignMechanicAsync_WhenOwnerAssignsSuccess_Returns200()
    {
        var bookingId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var garageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var mechanicMemberId = Guid.NewGuid();

        var garage = new GarageEntity
        {
            Id = garageId,
            OwnerId = ownerId,
            BusinessName = "G",
            Slug = "g",
            Status = GarageStatus.Active
        };
        var branch = new GarageBranch
        {
            Id = branchId,
            GarageId = garageId,
            Name = "Chi nhánh",
            Slug = "cn",
            Address = new(),
            WorkingHours = new(),
            Garage = garage
        };
        var reloaded = new Booking
        {
            Id = bookingId,
            GarageBranchId = branchId,
            UserId = userId,
            UserVehicleId = Guid.NewGuid(),
            MechanicId = mechanicMemberId,
            Status = BookingStatus.Confirmed,
            ScheduledAt = DateTime.UtcNow.AddDays(1),
            BookedTotalPrice = new Money { Amount = 100_000, Currency = "VND" },
            GarageBranch = branch,
            LineItems = [],
            StatusHistory = []
        };

        var m = new GarageUnitOfWorkMock();
        m.Bookings.Setup(r => r.GetAssignmentSnapshotAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookingAssignmentSnapshot(bookingId, BookingStatus.Pending, branchId, userId, reloaded.ScheduledAt));
        m.GarageBranches.Setup(r => r.GetGarageOwnerIdByBranchIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerId);
        m.Members.Setup(r => r.GetActiveMechanicForAssignmentAsync(mechanicMemberId, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((mechanicMemberId, "Thợ A"));
        m.Bookings.Setup(r => r.TryAssignMechanicPersistAsync(
                bookingId,
                mechanicMemberId,
                BookingStatus.Pending,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        m.Bookings.Setup(r => r.GetByIdWithDetailsAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reloaded);

        var sut = CreateSut(m);
        var result = await sut.AssignMechanicAsync(bookingId, ownerId, new AssignBookingRequest { GarageMemberId = mechanicMemberId });

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, EndpointMessages.Booking.AssignSuccess);
        result.Data!.Id.Should().Be(bookingId);
        result.Data.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task UpdateMechanicStatusAsync_WhenPersistReturnsFalse_Returns409()
    {
        var bookingId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var mechanicMemberId = Guid.NewGuid();
        var mechanicUserId = Guid.NewGuid();

        var booking = new Booking
        {
            Id = bookingId,
            GarageBranchId = branchId,
            UserId = Guid.NewGuid(),
            UserVehicleId = Guid.NewGuid(),
            MechanicId = mechanicMemberId,
            Status = BookingStatus.Confirmed,
            ScheduledAt = DateTime.UtcNow.AddHours(1),
            BookedTotalPrice = new Money { Amount = 100_000, Currency = "VND" },
            GarageBranch = new GarageBranch
            {
                Id = branchId,
                GarageId = Guid.NewGuid(),
                Name = "Chi nhánh",
                Slug = "chi-nhanh",
                Address = new(),
                WorkingHours = new(),
                Garage = new GarageEntity
                {
                    Id = Guid.NewGuid(),
                    OwnerId = Guid.NewGuid(),
                    BusinessName = "Garage",
                    Slug = "garage"
                }
            }
        };

        var m = new GarageUnitOfWorkMock();
        m.Bookings.Setup(r => r.GetByIdTrackedForMutationAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        m.Members.Setup(r => r.IsAssignedMechanicForUserAsync(mechanicMemberId, mechanicUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        m.Bookings.Setup(r => r.TryUpdateMechanicStatusPersistAsync(
                bookingId,
                mechanicMemberId,
                BookingStatus.Confirmed,
                BookingStatus.InProgress,
                mechanicUserId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = CreateSut(m);
        var result = await sut.UpdateMechanicStatusAsync(
            bookingId,
            mechanicUserId,
            new UpdateBookingMechanicStatusRequest { Status = BookingStatus.InProgress });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 409, EndpointMessages.Booking.AssignConcurrentlyModified);
        m.Bookings.Verify(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static BookingService CreateSut(GarageUnitOfWorkMock mock)
    {
        var vehicleClient = new Mock<IVehicleGarageClient>();
        vehicleClient
            .Setup(c => c.GetUserVehicleForBookingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookingVehicleSummary
            {
                UserVehicleId = Guid.NewGuid(),
                ModelName = "Model",
                BrandName = "Brand"
            });

        return new BookingService(
            NullLogger<BookingService>.Instance,
            mock.UnitOfWork.Object,
            Mock.Of<IPublishEndpoint>(),
            Mock.Of<IGarageIdentityContactClient>(),
            vehicleClient.Object);
    }
}
