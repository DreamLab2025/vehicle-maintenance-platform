using System.Linq.Expressions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Implements;
using Verendar.Garage.Domain.Entities;

namespace Verendar.Garage.Tests.Services;

public class ServiceCategoryServiceTests
{
    [Fact]
    public async Task GetByIdAsync_WhenNotFound_Returns404()
    {
        var id = Guid.NewGuid();
        var m = new GarageUnitOfWorkMock();
        m.ServiceCategories.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ServiceCategory, bool>>>()))
            .ReturnsAsync((ServiceCategory?)null);

        var sut = new ServiceCategoryService(NullLogger<ServiceCategoryService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetByIdAsync(id);

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 404, $"Không tìm thấy danh mục dịch vụ với id '{id}'.");
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ReturnsSuccess()
    {
        var m = new GarageUnitOfWorkMock();
        m.ServiceCategories.Setup(r => r.GetAllOrderedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ServiceCategory { Id = Guid.NewGuid(), Name = "Oil", Slug = "oil" }]);

        var sut = new ServiceCategoryService(NullLogger<ServiceCategoryService>.Instance, m.UnitOfWork.Object);
        var result = await sut.GetAllAsync();

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, "Lấy danh sách danh mục dịch vụ thành công");
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_WhenSlugExists_Returns409()
    {
        var m = new GarageUnitOfWorkMock();
        m.ServiceCategories.Setup(r => r.GetBySlugAsync("oil", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServiceCategory { Id = Guid.NewGuid(), Name = "Oil", Slug = "oil" });

        var sut = new ServiceCategoryService(NullLogger<ServiceCategoryService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateAsync(new CreateServiceCategoryRequest { Name = "Oil", Slug = "oil" });

        GarageServiceResponseAssert.AssertFailureEnvelope(result, 409, "Slug 'oil' đã được sử dụng.");
    }

    [Fact]
    public async Task CreateAsync_WhenSlugAvailable_Returns201()
    {
        var m = new GarageUnitOfWorkMock();
        m.ServiceCategories.Setup(r => r.GetBySlugAsync("wash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);
        m.ServiceCategories.Setup(r => r.AddAsync(It.IsAny<ServiceCategory>()))
            .ReturnsAsync((ServiceCategory c) => c);

        var sut = new ServiceCategoryService(NullLogger<ServiceCategoryService>.Instance, m.UnitOfWork.Object);
        var result = await sut.CreateAsync(new CreateServiceCategoryRequest { Name = "Wash", Slug = "wash" });

        GarageServiceResponseAssert.AssertCreatedEnvelope(result, "Tạo danh mục dịch vụ thành công");
        result.Data!.Slug.Should().Be("wash");
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryExists_Returns200()
    {
        var id = Guid.NewGuid();
        var entity = new ServiceCategory { Id = id, Name = "Oil", Slug = "oil" };
        var m = new GarageUnitOfWorkMock();
        m.ServiceCategories.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ServiceCategory, bool>>>()))
            .ReturnsAsync(entity);

        var sut = new ServiceCategoryService(NullLogger<ServiceCategoryService>.Instance, m.UnitOfWork.Object);
        var result = await sut.UpdateAsync(id, new UpdateServiceCategoryRequest { Name = "Oil Plus", DisplayOrder = 1 });

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, "Cập nhật danh mục dịch vụ thành công");
        result.Data!.Name.Should().Be("Oil Plus");
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryExists_SoftDeletesAndReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var entity = new ServiceCategory { Id = id, Name = "Oil", Slug = "oil" };
        var m = new GarageUnitOfWorkMock();
        m.ServiceCategories.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ServiceCategory, bool>>>()))
            .ReturnsAsync(entity);

        var sut = new ServiceCategoryService(NullLogger<ServiceCategoryService>.Instance, m.UnitOfWork.Object);
        var result = await sut.DeleteAsync(id);

        GarageServiceResponseAssert.AssertSuccessEnvelope(result, "Xóa danh mục dịch vụ thành công");
        entity.DeletedAt.Should().NotBeNull();
    }
}
