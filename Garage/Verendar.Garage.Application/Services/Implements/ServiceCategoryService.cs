using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Application.Services.Implements;

public class ServiceCategoryService(
    ILogger<ServiceCategoryService> logger,
    IUnitOfWork unitOfWork) : IServiceCategoryService
{
    private readonly ILogger<ServiceCategoryService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ApiResponse<List<ServiceCategoryResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var categories = await _unitOfWork.ServiceCategories.GetAllOrderedAsync(ct);
        return ApiResponse<List<ServiceCategoryResponse>>.SuccessResponse(
            categories.Select(c => c.ToResponse()).ToList(),
            "Lấy danh sách danh mục dịch vụ thành công");
    }

    public async Task<ApiResponse<ServiceCategoryResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _unitOfWork.ServiceCategories.FindOneAsync(
            c => c.Id == id && c.DeletedAt == null);

        if (category is null)
            return ApiResponse<ServiceCategoryResponse>.NotFoundResponse(
                $"Không tìm thấy danh mục dịch vụ với id '{id}'.");

        return ApiResponse<ServiceCategoryResponse>.SuccessResponse(
            category.ToResponse(), "Lấy thông tin danh mục dịch vụ thành công");
    }

    public async Task<ApiResponse<ServiceCategoryResponse>> CreateAsync(
        CreateServiceCategoryRequest request, CancellationToken ct = default)
    {
        var existing = await _unitOfWork.ServiceCategories.GetBySlugAsync(request.Slug, ct);
        if (existing is not null)
            return ApiResponse<ServiceCategoryResponse>.FailureResponse(
                $"Slug '{request.Slug}' đã được sử dụng.", 409);

        var entity = request.ToEntity();
        await _unitOfWork.ServiceCategories.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("CreateServiceCategory: created {Id} slug={Slug}", entity.Id, entity.Slug);

        return ApiResponse<ServiceCategoryResponse>.CreatedResponse(
            entity.ToResponse(), "Tạo danh mục dịch vụ thành công");
    }

    public async Task<ApiResponse<ServiceCategoryResponse>> UpdateAsync(
        Guid id, UpdateServiceCategoryRequest request, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.ServiceCategories.FindOneAsync(
            c => c.Id == id && c.DeletedAt == null);

        if (entity is null)
            return ApiResponse<ServiceCategoryResponse>.NotFoundResponse(
                $"Không tìm thấy danh mục dịch vụ với id '{id}'.");

        entity.UpdateFromRequest(request);
        entity.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("UpdateServiceCategory: updated {Id}", id);

        return ApiResponse<ServiceCategoryResponse>.SuccessResponse(
            entity.ToResponse(), "Cập nhật danh mục dịch vụ thành công");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _unitOfWork.ServiceCategories.FindOneAsync(
            c => c.Id == id && c.DeletedAt == null);

        if (entity is null)
            return ApiResponse<bool>.NotFoundResponse(
                $"Không tìm thấy danh mục dịch vụ với id '{id}'.");

        entity.DeletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("DeleteServiceCategory: soft deleted {Id}", id);

        return ApiResponse<bool>.SuccessResponse(true, "Xóa danh mục dịch vụ thành công");
    }
}
