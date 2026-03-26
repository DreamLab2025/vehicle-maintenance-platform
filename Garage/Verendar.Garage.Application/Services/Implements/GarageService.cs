using MassTransit;
using Verendar.Common.Shared;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Services.Interfaces;
using Verendar.Garage.Contracts.Events;

namespace Verendar.Garage.Application.Services.Implements;

public class GarageService(
    ILogger<GarageService> logger,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : IGarageService
{
    private readonly ILogger<GarageService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

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

    public async Task<ApiResponse<GarageDetailResponse>> GetMyGarageAsync(Guid ownerId, CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.GetWithBranchesAsync(g => g.OwnerId == ownerId, ct);

        if (garage is null)
            return ApiResponse<GarageDetailResponse>.NotFoundResponse("Bạn chưa đăng ký garage.");

        return ApiResponse<GarageDetailResponse>.SuccessResponse(
            garage.ToDetailResponse(), "Lấy thông tin garage thành công");
    }

    public async Task<ApiResponse<GarageDetailResponse>> GetGarageByIdAsync(Guid garageId, CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.GetWithBranchesAsync(g => g.Id == garageId, ct);

        if (garage is null)
            return ApiResponse<GarageDetailResponse>.NotFoundResponse($"Không tìm thấy garage.");

        return ApiResponse<GarageDetailResponse>.SuccessResponse(
            garage.ToDetailResponse(), "Lấy thông tin garage thành công");
    }

    public async Task<ApiResponse<GarageResponse>> UpdateGarageStatusAsync(
        Guid garageId, UpdateGarageStatusRequest request, Guid adminUserId, CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.FindOneAsync(
            g => g.Id == garageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<GarageResponse>.NotFoundResponse($"Không tìm thấy garage với id '{garageId}'.");

        if (!IsValidAdminTransition(garage.Status, request.Status))
            return ApiResponse<GarageResponse>.FailureResponse(
                $"Không thể chuyển trạng thái từ '{garage.Status}' sang '{request.Status}'.", 400);

        var fromStatus = garage.Status;
        garage.Status = request.Status;
        garage.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.StatusHistories.AddAsync(new GarageStatusHistory
        {
            GarageId = garageId,
            FromStatus = fromStatus,
            ToStatus = request.Status,
            ChangedByUserId = adminUserId,
            Reason = request.Reason,
            ChangedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("UpdateGarageStatus: garage {GarageId} {From} → {To} by admin {AdminId}",
            garageId, fromStatus, request.Status, adminUserId);

        await _publishEndpoint.Publish(new GarageStatusChangedEvent
        {
            GarageId = garage.Id,
            OwnerId = garage.OwnerId,
            BusinessName = garage.BusinessName,
            FromStatus = fromStatus.ToString(),
            ToStatus = request.Status.ToString(),
            Reason = request.Reason,
            ChangedAt = DateTime.UtcNow
        }, ct);

        return ApiResponse<GarageResponse>.SuccessResponse(garage.ToResponse(), "Cập nhật trạng thái garage thành công");
    }

    public async Task<ApiResponse<GarageResponse>> UpdateGarageInfoAsync(
        Guid garageId, Guid ownerId, GarageRequest request, CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.FindOneAsync(
            g => g.Id == garageId && g.OwnerId == ownerId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<GarageResponse>.NotFoundResponse("Không tìm thấy garage.");

        if (garage.Status == GarageStatus.Active || garage.Status == GarageStatus.Suspended)
            return ApiResponse<GarageResponse>.FailureResponse(
                "Không thể chỉnh sửa thông tin garage khi đang ở trạng thái Active hoặc Suspended.", 400);

        garage.UpdateFromRequest(request);
        garage.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("UpdateGarageInfo: garage {GarageId} updated by owner {OwnerId}", garageId, ownerId);

        return ApiResponse<GarageResponse>.SuccessResponse(garage.ToResponse(), "Cập nhật thông tin garage thành công");
    }

    public async Task<ApiResponse<GarageResponse>> ResubmitGarageAsync(
        Guid garageId, Guid ownerId, CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.FindOneAsync(
            g => g.Id == garageId && g.OwnerId == ownerId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<GarageResponse>.NotFoundResponse("Không tìm thấy garage.");

        if (garage.Status != GarageStatus.Rejected)
            return ApiResponse<GarageResponse>.FailureResponse(
                "Chỉ có thể nộp lại khi garage đang ở trạng thái Rejected.", 400);

        garage.Status = GarageStatus.Pending;
        garage.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.StatusHistories.AddAsync(new GarageStatusHistory
        {
            GarageId = garageId,
            FromStatus = GarageStatus.Rejected,
            ToStatus = GarageStatus.Pending,
            ChangedByUserId = ownerId,
            Reason = "Owner resubmitted for review",
            ChangedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("ResubmitGarage: garage {GarageId} resubmitted by owner {OwnerId}", garageId, ownerId);

        return ApiResponse<GarageResponse>.SuccessResponse(garage.ToResponse(), "Nộp lại hồ sơ thành công");
    }

    private static bool IsValidAdminTransition(GarageStatus from, GarageStatus to) => (from, to) switch
    {
        (GarageStatus.Pending, GarageStatus.Active) => true,
        (GarageStatus.Pending, GarageStatus.Rejected) => true,
        (GarageStatus.Active, GarageStatus.Suspended) => true,
        (GarageStatus.Suspended, GarageStatus.Active) => true,
        (GarageStatus.Suspended, GarageStatus.Rejected) => true,
        _ => false
    };

    public async Task<ApiResponse<GarageResponse>> CreateGarageAsync(Guid ownerId, GarageRequest request)
    {
        var existing = await _unitOfWork.Garages.FindOneAsync(g => g.OwnerId == ownerId && g.DeletedAt == null);
        if (existing != null)
        {
            if (existing.Status != GarageStatus.Rejected)
            {
                _logger.LogWarning("CreateGarage: owner {OwnerId} already has a garage with status {Status}", ownerId, existing.Status);
                return ApiResponse<GarageResponse>.ConflictResponse(
                    "Tài khoản đã có garage đăng ký. Nếu bị từ chối, hãy chỉnh sửa và nộp lại.");
            }

            // Garage bị Rejected — hướng dẫn dùng flow edit + resubmit thay vì tạo mới
            return ApiResponse<GarageResponse>.FailureResponse(
                "Garage của bạn đã bị từ chối. Hãy chỉnh sửa thông tin qua PUT /api/v1/garages/{id} và nộp lại qua PATCH /api/v1/garages/{id}/resubmit.", 400);
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
