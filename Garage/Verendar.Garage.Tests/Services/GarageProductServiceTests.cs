using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Implements;
using Verendar.Garage.Domain.Entities;
using Verendar.Garage.Domain.Enums;
using Verendar.Garage.Domain.ValueObjects;
using GarageEntity = Verendar.Garage.Domain.Entities.Garage;

namespace Verendar.Garage.Tests.Services;

public class GarageProductServiceTests
{
    [Fact]
    public async Task GetProductByIdAsync_WhenNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        var m = new GarageUnitOfWorkMock();
        m.GarageProducts.Setup(r => r.GetByIdWithInstallationAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GarageProduct?)null);
        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);

        var result = await sut.GetProductByIdAsync(id);

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, string.Format(EndpointMessages.Product.NotFoundByIdFormat, id));
    }

    [Fact]
    public async Task CreateProductAsync_WhenInstallationServiceInvalid_Returns422()
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
        m.GarageServices.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Domain.Entities.GarageService, bool>>>()))
            .ReturnsAsync((Domain.Entities.GarageService?)null);

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateProductAsync(branchId, ownerId, new CreateGarageProductRequest
        {
            Name = "Oil Filter",
            MaterialPrice = new MoneyDto { Amount = 200, Currency = "VND" },
            InstallationServiceId = Guid.NewGuid()
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 422, EndpointMessages.Product.InstallationServiceInvalid);
    }

    [Fact]
    public async Task CreateProductAsync_WhenOwnerAndValidInstallationService_Returns201()
    {
        var branchId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var installationServiceId = Guid.NewGuid();

        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.GarageServices.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Domain.Entities.GarageService, bool>>>()))
            .ReturnsAsync(new Domain.Entities.GarageService
            {
                Id = installationServiceId,
                GarageBranchId = branchId,
                Name = "Install",
                LaborPrice = new Money { Amount = 50, Currency = "VND" }
            });
        m.GarageProducts.Setup(r => r.AddAsync(It.IsAny<GarageProduct>()))
            .ReturnsAsync((GarageProduct p) => { p.Id = productId; return p; });
        m.GarageProducts.Setup(r => r.GetByIdWithInstallationAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GarageProduct
            {
                Id = productId,
                GarageBranchId = branchId,
                Name = "Oil Filter",
                MaterialPrice = new Money { Amount = 200, Currency = "VND" }
            });

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateProductAsync(branchId, ownerId, new CreateGarageProductRequest
        {
            Name = "Oil Filter",
            MaterialPrice = new MoneyDto { Amount = 200, Currency = "VND" },
            InstallationServiceId = installationServiceId
        });

        GarageServiceResponseAssert.AssertCreatedEnvelope(result, EndpointMessages.Product.CreateSuccess);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenOwner_Returns200()
    {
        var ownerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var product = new GarageProduct
        {
            Id = Guid.NewGuid(),
            GarageBranchId = branchId,
            Name = "Oil Filter",
            MaterialPrice = new Money { Amount = 200, Currency = "VND" }
        };
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };

        var m = new GarageUnitOfWorkMock();
        m.GarageProducts.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageProduct, bool>>>()))
            .ReturnsAsync(product);
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.DeleteProductAsync(product.Id, ownerId);

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, EndpointMessages.Product.DeleteSuccess);
        product.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateProductStatusAsync_WhenProductNotFound_Returns404()
    {
        var m = new GarageUnitOfWorkMock();
        m.GarageProducts.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageProduct, bool>>>()))
            .ReturnsAsync((GarageProduct?)null);
        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);

        var result = await sut.UpdateProductStatusAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateGarageProductStatusRequest { Status = ProductStatus.Inactive });

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetProductsByBranchAsync_WhenBranchNotFound_Returns404()
    {
        var branchId = Guid.NewGuid();
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync((GarageBranch?)null);

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetProductsByBranchAsync(new GarageProductQueryRequest { BranchId = branchId, ActiveOnly = false });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, string.Format(EndpointMessages.BranchManager.BranchNotFoundByIdFormat, branchId));
    }

    [Fact]
    public async Task GetProductsByBranchAsync_WhenBranchExists_ReturnsPagedSuccess()
    {
        var branchId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var product = new GarageProduct
        {
            Id = Guid.NewGuid(),
            GarageBranchId = branchId,
            Name = "P",
            MaterialPrice = new Money { Amount = 1, Currency = "VND" }
        };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.GarageProducts.Setup(r => r.GetPagedByBranchIdAsync(
                branchId,
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<decimal?>(),
                It.IsAny<decimal?>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(([product], 1));

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetProductsByBranchAsync(new GarageProductQueryRequest { BranchId = branchId, ActiveOnly = false });

        GarageServiceResponseAssert.AssertPagedSuccessEnvelope(result, EndpointMessages.Product.ListSuccess, 1, 1);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenFound_Returns200()
    {
        var id = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var m = new GarageUnitOfWorkMock();
        m.GarageProducts.Setup(r => r.GetByIdWithInstallationAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GarageProduct
            {
                Id = id,
                GarageBranchId = branchId,
                Name = "P",
                MaterialPrice = new Money { Amount = 1, Currency = "VND" }
            });

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetProductByIdAsync(id);

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, EndpointMessages.Product.GetSuccess);
    }

    [Fact]
    public async Task CreateProductAsync_WhenBranchNotFound_Returns404()
    {
        var branchId = Guid.NewGuid();
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync((GarageBranch?)null);

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateProductAsync(branchId, Guid.NewGuid(), new CreateGarageProductRequest
        {
            Name = "P",
            MaterialPrice = new MoneyDto { Amount = 1, Currency = "VND" }
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, string.Format(EndpointMessages.BranchManager.BranchNotFoundByIdFormat, branchId));
    }

    [Fact]
    public async Task CreateProductAsync_WhenGarageNotFound_Returns404()
    {
        var branchId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync((GarageEntity?)null);

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateProductAsync(branchId, Guid.NewGuid(), new CreateGarageProductRequest
        {
            Name = "P",
            MaterialPrice = new MoneyDto { Amount = 1, Currency = "VND" }
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, EndpointMessages.Member.GarageNotFound);
    }

    [Fact]
    public async Task CreateProductAsync_WhenNotOwnerOrManager_Returns403()
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

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateProductAsync(branchId, Guid.NewGuid(), new CreateGarageProductRequest
        {
            Name = "P",
            MaterialPrice = new MoneyDto { Amount = 1, Currency = "VND" }
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 403, EndpointMessages.BranchManager.ForbiddenManageProducts);
    }

    [Fact]
    public async Task CreateProductAsync_WhenManager_Returns201()
    {
        var branchId = Guid.NewGuid();
        var managerUserId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = Guid.NewGuid(), BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.Members.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageMember, bool>>>()))
            .ReturnsAsync(new GarageMember { UserId = managerUserId, GarageBranchId = branchId, Role = MemberRole.Manager });
        m.GarageProducts.Setup(r => r.AddAsync(It.IsAny<GarageProduct>()))
            .ReturnsAsync((GarageProduct p) => { p.Id = productId; return p; });
        m.GarageProducts.Setup(r => r.GetByIdWithInstallationAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GarageProduct
            {
                Id = productId,
                GarageBranchId = branchId,
                Name = "P",
                MaterialPrice = new Money { Amount = 1, Currency = "VND" }
            });

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateProductAsync(branchId, managerUserId, new CreateGarageProductRequest
        {
            Name = "P",
            MaterialPrice = new MoneyDto { Amount = 1, Currency = "VND" }
        });

        GarageServiceResponseAssert.AssertCreatedEnvelope(result, EndpointMessages.Product.CreateSuccess);
    }

    [Fact]
    public async Task CreateProductAsync_WhenNoInstallationService_Returns201()
    {
        var branchId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.GarageProducts.Setup(r => r.AddAsync(It.IsAny<GarageProduct>()))
            .ReturnsAsync((GarageProduct p) => { p.Id = productId; return p; });
        m.GarageProducts.Setup(r => r.GetByIdWithInstallationAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GarageProduct
            {
                Id = productId,
                GarageBranchId = branchId,
                Name = "P",
                MaterialPrice = new Money { Amount = 1, Currency = "VND" }
            });

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateProductAsync(branchId, ownerId, new CreateGarageProductRequest
        {
            Name = "P",
            MaterialPrice = new MoneyDto { Amount = 1, Currency = "VND" }
        });

        GarageServiceResponseAssert.AssertCreatedEnvelope(result, EndpointMessages.Product.CreateSuccess);
    }

    [Fact]
    public async Task UpdateProductAsync_WhenOwner_Returns200()
    {
        var productId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var product = new GarageProduct
        {
            Id = productId,
            GarageBranchId = branchId,
            Name = "Old",
            MaterialPrice = new Money { Amount = 1, Currency = "VND" }
        };
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageProducts.SetupSequence(r => r.GetByIdWithInstallationAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product)
            .ReturnsAsync(new GarageProduct
            {
                Id = productId,
                GarageBranchId = branchId,
                Name = "New",
                MaterialPrice = new Money { Amount = 2, Currency = "VND" }
            });
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.UpdateProductAsync(productId, ownerId, new UpdateGarageProductRequest
        {
            Name = "New",
            MaterialPrice = new MoneyDto { Amount = 2, Currency = "VND" }
        });

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, EndpointMessages.Product.UpdateSuccess);
    }

    [Fact]
    public async Task UpdateProductAsync_WhenInstallationInvalid_Returns422()
    {
        var productId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var product = new GarageProduct
        {
            Id = productId,
            GarageBranchId = branchId,
            Name = "P",
            MaterialPrice = new Money { Amount = 1, Currency = "VND" }
        };
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageProducts.Setup(r => r.GetByIdWithInstallationAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.GarageServices.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<Domain.Entities.GarageService, bool>>>()))
            .ReturnsAsync((Domain.Entities.GarageService?)null);

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.UpdateProductAsync(productId, ownerId, new UpdateGarageProductRequest
        {
            Name = "P",
            MaterialPrice = new MoneyDto { Amount = 1, Currency = "VND" },
            InstallationServiceId = Guid.NewGuid()
        });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 422, EndpointMessages.Product.InstallationServiceInvalid);
    }

    [Fact]
    public async Task UpdateProductStatusAsync_WhenOwner_Returns200()
    {
        var productId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var product = new GarageProduct
        {
            Id = productId,
            GarageBranchId = branchId,
            Name = "P",
            MaterialPrice = new Money { Amount = 1, Currency = "VND" },
            Status = ProductStatus.Active
        };
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = ownerId, BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageProducts.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageProduct, bool>>>()))
            .ReturnsAsync(product);
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.GarageProducts.Setup(r => r.GetByIdWithInstallationAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GarageProduct
            {
                Id = productId,
                GarageBranchId = branchId,
                Name = "P",
                MaterialPrice = new Money { Amount = 1, Currency = "VND" },
                Status = ProductStatus.Inactive
            });

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.UpdateProductStatusAsync(productId, ownerId, new UpdateGarageProductStatusRequest { Status = ProductStatus.Inactive });

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, EndpointMessages.Product.UpdateStatusSuccess);
    }

    [Fact]
    public async Task UpdateProductStatusAsync_WhenForbidden_Returns403()
    {
        var productId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var product = new GarageProduct
        {
            Id = productId,
            GarageBranchId = branchId,
            Name = "P",
            MaterialPrice = new Money { Amount = 1, Currency = "VND" }
        };
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = Guid.NewGuid(), BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageProducts.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageProduct, bool>>>()))
            .ReturnsAsync(product);
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.Members.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageMember, bool>>>()))
            .ReturnsAsync((GarageMember?)null);

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.UpdateProductStatusAsync(productId, Guid.NewGuid(), new UpdateGarageProductStatusRequest { Status = ProductStatus.Inactive });

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Message.Should().Be(EndpointMessages.BranchManager.ForbiddenManageProducts);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenNotFound_Returns404()
    {
        var m = new GarageUnitOfWorkMock();
        m.GarageProducts.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageProduct, bool>>>()))
            .ReturnsAsync((GarageProduct?)null);

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.DeleteProductAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenForbidden_Returns403()
    {
        var productId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var product = new GarageProduct
        {
            Id = productId,
            GarageBranchId = branchId,
            Name = "P",
            MaterialPrice = new Money { Amount = 1, Currency = "VND" }
        };
        var branch = new GarageBranch { Id = branchId, GarageId = Guid.NewGuid(), Name = "B", Slug = "b", Address = new(), WorkingHours = new() };
        var garage = new GarageEntity { Id = branch.GarageId, OwnerId = Guid.NewGuid(), BusinessName = "G", Slug = "g" };
        var m = new GarageUnitOfWorkMock();
        m.GarageProducts.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageProduct, bool>>>()))
            .ReturnsAsync(product);
        m.GarageBranches.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageBranch, bool>>>()))
            .ReturnsAsync(branch);
        m.Garages.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageEntity, bool>>>()))
            .ReturnsAsync(garage);
        m.Members.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<GarageMember, bool>>>()))
            .ReturnsAsync((GarageMember?)null);

        var sut = new GarageProductService(NullLogger<GarageProductService>.Instance, m.UnitOfWork.Object);
        var result = await sut.DeleteProductAsync(productId, Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Message.Should().Be(EndpointMessages.BranchManager.ForbiddenManageProducts);
    }
}
