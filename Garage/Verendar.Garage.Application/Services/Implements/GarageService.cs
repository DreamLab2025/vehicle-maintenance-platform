using Verendar.Common.Shared;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Application.Services.Implements;

public class GarageService(
    ILogger<GarageService> logger,
    IUnitOfWork unitOfWork) : IGarageService
{
    private readonly ILogger<GarageService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ApiResponse<List<GarageResponse>>> GetGaragesAsync(GarageFilterRequest request)
    {
        request.Normalize();

        var (items, totalCount) = await _unitOfWork.Garages.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            filter: request.Status.HasValue ? g => g.Status == request.Status.Value : null,
            orderBy: request.IsDescending == false
                ? q => q.OrderBy(g => g.CreatedAt)
                : q => q.OrderByDescending(g => g.CreatedAt));

        return ApiResponse<GarageResponse>.SuccessPagedResponse(
            items.Select(g => g.ToResponse()).ToList(),
            totalCount,
            request.PageNumber,
            request.PageSize,
            "Lấy danh sách garage thành công");
    }

    public async Task<ApiResponse<GarageResponse>> CreateGarageAsync(Guid ownerId, GarageRequest request)
    {
        var existing = await _unitOfWork.Garages.FindOneAsync(g => g.OwnerId == ownerId);
        if (existing != null)
        {
            _logger.LogWarning("CreateGarage: owner {OwnerId} already has a garage", ownerId);
            return ApiResponse<GarageResponse>.ConflictResponse("Tài khoản đã có garage đăng ký");
        }

        var garage = request.ToEntity(ownerId);

        garage.Slug = await SlugUtils.EnsureUniqueAsync(
            SlugUtils.ToSlug(request.BusinessName, 110),
            async s => (await _unitOfWork.Garages.FindOneAsync(g => g.Slug == s)) != null,
            maxLength: 110);

        await _unitOfWork.Garages.AddAsync(garage);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("CreateGarage: created garage {GarageId} for owner {OwnerId}", garage.Id, ownerId);

        return ApiResponse<GarageResponse>.CreatedResponse(garage.ToResponse(), "Đăng ký garage thành công");
    }
}
