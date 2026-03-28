using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Common.Shared;
using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Implements;
using Verendar.Garage.Domain.Entities;
using Verendar.Garage.Domain.Enums;
using GarageEntity = Verendar.Garage.Domain.Entities.Garage;

namespace Verendar.Garage.Tests.Services;

public class GarageBundleServiceTests
{
    [Fact]
    public async Task GetBundleByIdAsync_WhenNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        var m = new GarageUnitOfWorkMock();
        m.GarageBundles.Setup(r => r.GetByIdWithItemsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GarageBundle?)null);

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetBundleByIdAsync(id);

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, string.Format(EndpointMessages.Bundle.NotFoundByIdFormat, id));
    }

    [Fact]
    public async Task CreateBundleAsync_WhenNoItems_Returns422()
    {
        var branchId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateBundleAsync(branchId, ownerId, new CreateGarageBundleRequest
        {
            Name = "Combo A",
            Items = []
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 422, EndpointMessages.Bundle.EmptyItems);
    }

    [Fact]
    public async Task CreateBundleAsync_WhenOwnerAndValidItems_Returns201()
    {
        var branchId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var bundleId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var productId = Guid.NewGuid();

        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.GarageProducts.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageProduct, bool>>>()))
            .ReturnsAsync(new GarageProduct
            {
                Id = productId,
                GarageBranchId = branchId,
                Name = "Oil Filter",
                Status = ProductStatus.Active,
                MaterialPrice = new() { Amount = 100, Currency = "VND" }
            });
        m.GarageBundles.Setup(r => r.AddAsync(It.IsAny<GarageBundle>()))
            .ReturnsAsync((GarageBundle b) => { b.Id = bundleId; return b; });
        m.GarageBundles.Setup(r => r.GetByIdWithItemsAsync(bundleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GarageBundle
            {
                Id = bundleId,
                GarageBranchId = branchId,
                Name = "Combo A",
                Items = []
            });

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateBundleAsync(branchId, ownerId, new CreateGarageBundleRequest
        {
            Name = "Combo A",
            Items = [new BundleItemRequest { ProductId = productId }]
        });

        GarageServiceResponseAssert.AssertCreatedEnvelope(result, EndpointMessages.Bundle.CreateSuccess);
    }

    [Fact]
    public async Task DeleteBundleAsync_WhenOwner_Returns200()
    {
        var ownerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var bundle = new GarageBundle { Id = Guid.NewGuid(), GarageBranchId = branchId, Name = "Combo A" };
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };

        var m = new GarageUnitOfWorkMock();
        m.GarageBundles.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBundle, bool>>>()))
            .ReturnsAsync(bundle);
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.DeleteBundleAsync(bundle.Id, ownerId);

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, EndpointMessages.Bundle.DeleteSuccess);
        bundle.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateBundleStatusAsync_WhenBundleNotFound_Returns404()
    {
        var m = new GarageUnitOfWorkMock();
        m.GarageBundles.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBundle, bool>>>()))
            .ReturnsAsync((GarageBundle?)null);
        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);

        var result = await sut.UpdateBundleStatusAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateGarageBundleStatusRequest { Status = ProductStatus.Inactive });

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetBundlesByBranchAsync_WhenBranchNotFound_Returns404()
    {
        var branchId = Guid.NewGuid();
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync((GarageBranch?)null);

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetBundlesByBranchAsync(branchId, false, new PaginationRequest());

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, string.Format(EndpointMessages.BranchManager.BranchNotFoundByIdFormat, branchId));
    }

    [Fact]
    public async Task GetBundlesByBranchAsync_WhenBranchExists_ReturnsPagedSuccess()
    {
        var branchId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var bundle = new GarageBundle
        {
            Id = Guid.NewGuid(),
            GarageBranchId = branchId,
            Name = "Combo",
            Items = []
        };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.GarageBundles.Setup(r => r.GetPagedByBranchIdAsync(
                branchId,
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(([bundle], 1));

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetBundlesByBranchAsync(branchId, false, new PaginationRequest());

        GarageServiceResponseAssert.AssertPagedSuccessEnvelope(result, EndpointMessages.Bundle.ListSuccess, 1, 1);
    }

    [Fact]
    public async Task GetBundleByIdAsync_WhenFound_Returns200()
    {
        var id = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var m = new GarageUnitOfWorkMock();
        m.GarageBundles.Setup(r => r.GetByIdWithItemsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GarageBundle { Id = id, GarageBranchId = branchId, Name = "Combo", Items = [] });

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetBundleByIdAsync(id);

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, EndpointMessages.Bundle.GetSuccess);
    }

    [Fact]
    public async Task CreateBundleAsync_WhenItemHasBothProductAndService_Returns422()
    {
        var branchId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateBundleAsync(branchId, ownerId, new CreateGarageBundleRequest
        {
            Name = "Combo",
            Items = [new BundleItemRequest { ProductId = Guid.NewGuid(), ServiceId = Guid.NewGuid() }]
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 422, string.Format(EndpointMessages.Bundle.ItemSpecifyProductOrServiceFormat, 1));
    }

    [Fact]
    public async Task CreateBundleAsync_WhenProductNotFound_Returns422()
    {
        var branchId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.GarageProducts.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageProduct, bool>>>()))
            .ReturnsAsync((GarageProduct?)null);

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateBundleAsync(branchId, ownerId, new CreateGarageBundleRequest
        {
            Name = "Combo",
            Items = [new BundleItemRequest { ProductId = Guid.NewGuid() }]
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 422, string.Format(EndpointMessages.Bundle.ItemProductNotInBranchFormat, 1));
    }

    [Fact]
    public async Task CreateBundleAsync_WhenServiceInactive_Returns422()
    {
        var branchId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.GarageServices.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Domain.Entities.GarageService, bool>>>()))
            .ReturnsAsync(new Domain.Entities.GarageService
            {
                Id = serviceId,
                GarageBranchId = branchId,
                Name = "Wash",
                Status = ProductStatus.Inactive,
                LaborPrice = new() { Amount = 1, Currency = "VND" }
            });

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateBundleAsync(branchId, ownerId, new CreateGarageBundleRequest
        {
            Name = "Combo",
            Items = [new BundleItemRequest { ServiceId = serviceId }]
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 422, string.Format(EndpointMessages.Bundle.ItemServiceUnavailableFormat, 1, "Wash"));
    }

    [Fact]
    public async Task CreateBundleAsync_WhenForbidden_Returns403()
    {
        var branchId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = Guid.NewGuid(), BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.Members.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageMember, bool>>>()))
            .ReturnsAsync((GarageMember?)null);

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateBundleAsync(branchId, Guid.NewGuid(), new CreateGarageBundleRequest
        {
            Name = "Combo",
            Items = [new BundleItemRequest { ProductId = Guid.NewGuid() }]
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 403, EndpointMessages.BranchManager.ForbiddenManageBundles);
    }

    [Fact]
    public async Task UpdateBundleAsync_WhenOwner_Returns200()
    {
        var bundleId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var oldItemId = Guid.NewGuid();
        var bundle = new GarageBundle
        {
            Id = bundleId,
            GarageBranchId = branchId,
            Name = "Old",
            Items =
            [
                new GarageBundleItem { Id = oldItemId, GarageBundleId = bundleId, ProductId = productId }
            ]
        };
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageBundles.SetupSequence(r => r.GetByIdWithItemsAsync(bundleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle)
            .ReturnsAsync(new GarageBundle { Id = bundleId, GarageBranchId = branchId, Name = "New", Items = [] });
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.GarageProducts.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageProduct, bool>>>()))
            .ReturnsAsync(new GarageProduct
            {
                Id = productId,
                GarageBranchId = branchId,
                Name = "Oil",
                Status = ProductStatus.Active,
                MaterialPrice = new() { Amount = 100, Currency = "VND" }
            });

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.UpdateBundleAsync(bundleId, ownerId, new UpdateGarageBundleRequest
        {
            Name = "New",
            Items = [new BundleItemRequest { ProductId = productId }]
        });

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, EndpointMessages.Bundle.UpdateSuccess);
    }

    [Fact]
    public async Task UpdateBundleAsync_WhenNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        var m = new GarageUnitOfWorkMock();
        m.GarageBundles.Setup(r => r.GetByIdWithItemsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GarageBundle?)null);

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.UpdateBundleAsync(id, Guid.NewGuid(), new UpdateGarageBundleRequest { Name = "N", Items = [] });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, string.Format(EndpointMessages.Bundle.NotFoundByIdFormat, id));
    }

    [Fact]
    public async Task UpdateBundleStatusAsync_WhenOwner_Returns200()
    {
        var bundleId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var bundle = new GarageBundle { Id = bundleId, GarageBranchId = branchId, Name = "Combo", Status = ProductStatus.Active };
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageBundles.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBundle, bool>>>()))
            .ReturnsAsync(bundle);
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.GarageBundles.Setup(r => r.GetByIdWithItemsAsync(bundleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GarageBundle { Id = bundleId, GarageBranchId = branchId, Name = "Combo", Status = ProductStatus.Inactive, Items = [] });

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.UpdateBundleStatusAsync(bundleId, ownerId, new UpdateGarageBundleStatusRequest { Status = ProductStatus.Inactive });

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, EndpointMessages.Bundle.UpdateStatusSuccess);
    }

    [Fact]
    public async Task DeleteBundleAsync_WhenNotFound_Returns404()
    {
        var m = new GarageUnitOfWorkMock();
        m.GarageBundles.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBundle, bool>>>()))
            .ReturnsAsync((GarageBundle?)null);

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.DeleteBundleAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteBundleAsync_WhenForbidden_Returns403()
    {
        var bundleId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var bundle = new GarageBundle { Id = bundleId, GarageBranchId = branchId, Name = "Combo" };
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = Guid.NewGuid(), BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageBundles.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBundle, bool>>>()))
            .ReturnsAsync(bundle);
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.Members.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageMember, bool>>>()))
            .ReturnsAsync((GarageMember?)null);

        var sut = new GarageBundleService(NullLogger<GarageBundleService>.Instance, m.UnitOfWork.Object);
        var result = await sut.DeleteBundleAsync(bundleId, Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Message.Should().Be(EndpointMessages.BranchManager.ForbiddenManageBundles);
    }
}
