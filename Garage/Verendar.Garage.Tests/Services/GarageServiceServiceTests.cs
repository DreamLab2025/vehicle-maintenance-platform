using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Common.Shared;
using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Implements;
using Verendar.Garage.Domain.Entities;
using Verendar.Garage.Domain.Enums;
using Verendar.Garage.Domain.ValueObjects;
using GarageEntity = Verendar.Garage.Domain.Entities.Garage;

namespace Verendar.Garage.Tests.Services;

public class GarageServiceServiceTests
{
    [Fact]
    public async Task GetServicesByBranchAsync_WhenBranchNotFound_Returns404()
    {
        var branchId = Guid.NewGuid();
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync((GarageBranch?)null);

        var sut = new GarageServiceService(NullLogger<GarageServiceService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetServicesByBranchAsync(branchId, false, new PaginationRequest());

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, string.Format(EndpointMessages.BranchManager.BranchNotFoundByIdFormat, branchId));
    }

    [Fact]
    public async Task CreateServiceAsync_WhenOwnerAndValidCategory_Returns201()
    {
        var branchId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var categoryId = Guid.NewGuid();

        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.ServiceCategories.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ServiceCategory, bool>>>()))
            .ReturnsAsync(new ServiceCategory { Id = categoryId, Name = "Oil", Slug = "oil" });
        m.GarageServices.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.GarageService>()))
            .ReturnsAsync((Domain.Entities.GarageService s) => { s.Id = serviceId; return s; });
        m.GarageServices.Setup(r => r.GetByIdWithCategoryAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.GarageService
            {
                Id = serviceId,
                GarageBranchId = branchId,
                Name = "Wash",
                LaborPrice = new Money { Amount = 100, Currency = "VND" }
            });

        var sut = new GarageServiceService(NullLogger<GarageServiceService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateServiceAsync(branchId, ownerId, new CreateGarageServiceRequest
        {
            Name = "Wash",
            LaborPrice = new MoneyDto { Amount = 100, Currency = "VND" },
            ServiceCategoryId = categoryId
        });

        GarageServiceResponseAssert.AssertCreatedEnvelope(result, EndpointMessages.OfferedServices.CreateSuccess);
    }

    [Fact]
    public async Task GetServiceByIdAsync_WhenNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        var m = new GarageUnitOfWorkMock();
        m.GarageServices.Setup(r => r.GetByIdWithCategoryAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.GarageService?)null);

        var sut = new GarageServiceService(NullLogger<GarageServiceService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetServiceByIdAsync(id);

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, string.Format(EndpointMessages.OfferedServices.NotFoundByIdFormat, id));
    }

    [Fact]
    public async Task UpdateServiceAsync_WhenOwner_Returns200()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var service = new Domain.Entities.GarageService
        {
            Id = id,
            GarageBranchId = branchId,
            Name = "Old",
            LaborPrice = new Money { Amount = 100, Currency = "VND" }
        };

        var m = new GarageUnitOfWorkMock();
        m.GarageServices.Setup(r => r.GetByIdWithCategoryForUpdateAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        m.GarageServices.Setup(r => r.GetByIdWithCategoryAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.GarageService
            {
                Id = id,
                GarageBranchId = branchId,
                Name = "New",
                LaborPrice = new Money { Amount = 200, Currency = "VND" }
            });
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);

        var sut = new GarageServiceService(NullLogger<GarageServiceService>.Instance, m.UnitOfWork.Object);
        var result = await sut.UpdateServiceAsync(id, ownerId, new UpdateGarageServiceRequest
        {
            Name = "New",
            LaborPrice = new MoneyDto { Amount = 200, Currency = "VND" }
        });

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, EndpointMessages.OfferedServices.UpdateSuccess);
    }

    [Fact]
    public async Task UpdateServiceStatusAsync_WhenOwner_Returns200()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var service = new Domain.Entities.GarageService
        {
            Id = id,
            GarageBranchId = branchId,
            Name = "Wash",
            LaborPrice = new Money { Amount = 100, Currency = "VND" },
            Status = ProductStatus.Active
        };

        var m = new GarageUnitOfWorkMock();
        m.GarageServices.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Domain.Entities.GarageService, bool>>>()))
            .ReturnsAsync(service);
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.GarageServices.Setup(r => r.GetByIdWithCategoryAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var sut = new GarageServiceService(NullLogger<GarageServiceService>.Instance, m.UnitOfWork.Object);
        var result = await sut.UpdateServiceStatusAsync(id, ownerId, new UpdateGarageServiceStatusRequest { Status = ProductStatus.Inactive });

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, EndpointMessages.OfferedServices.UpdateStatusSuccess);
        service.Status.Should().Be(ProductStatus.Inactive);
    }

    [Fact]
    public async Task DeleteServiceAsync_WhenRequesterHasNoAccess_Returns403()
    {
        var id = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var service = new Domain.Entities.GarageService
        {
            Id = id,
            GarageBranchId = branchId,
            Name = "Wash",
            LaborPrice = new Money { Amount = 100, Currency = "VND" }
        };
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = Guid.NewGuid(), BusinessName = "G", Slug = "g" };

        var m = new GarageUnitOfWorkMock();
        m.GarageServices.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Domain.Entities.GarageService, bool>>>()))
            .ReturnsAsync(service);
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.Members.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageMember, bool>>>()))
            .ReturnsAsync((GarageMember?)null);

        var sut = new GarageServiceService(NullLogger<GarageServiceService>.Instance, m.UnitOfWork.Object);
        var result = await sut.DeleteServiceAsync(id, Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Message.Should().Be(EndpointMessages.BranchManager.ForbiddenManageServices);
    }
}
