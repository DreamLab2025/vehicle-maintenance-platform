using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Application.Services.Implements;

public class GarageBundleService(
    ILogger<GarageBundleService> logger,
    IUnitOfWork unitOfWork) : IGarageBundleService
{
    private readonly ILogger<GarageBundleService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ApiResponse<List<GarageBundleListItemResponse>>> GetBundlesByBranchAsync(
        Guid branchId, bool activeOnly, PaginationRequest pagination, CancellationToken ct = default)
    {
        pagination.Normalize();

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == branchId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<List<GarageBundleListItemResponse>>.NotFoundResponse(
                $"Không tìm thấy chi nhánh với id '{branchId}'.");

        var (items, totalCount) = await _unitOfWork.GarageBundles.GetPagedByBranchIdAsync(
            branchId, activeOnly, pagination.PageNumber, pagination.PageSize, ct);

        return ApiResponse<List<GarageBundleListItemResponse>>.SuccessPagedResponse(
            items.Select(b => b.ToListItemResponse()).ToList(),
            totalCount,
            pagination.PageNumber,
            pagination.PageSize,
            "Lấy danh sách combo thành công");
    }

    public async Task<ApiResponse<GarageBundleResponse>> GetBundleByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var bundle = await _unitOfWork.GarageBundles.GetByIdWithItemsAsync(id, ct);
        if (bundle is null)
            return ApiResponse<GarageBundleResponse>.NotFoundResponse(
                $"Không tìm thấy combo với id '{id}'.");

        return ApiResponse<GarageBundleResponse>.SuccessResponse(
            bundle.ToResponse(), "Lấy thông tin combo thành công");
    }

    public async Task<ApiResponse<GarageBundleResponse>> CreateBundleAsync(
        Guid branchId, Guid requestingUserId, CreateGarageBundleRequest request, CancellationToken ct = default)
    {
        var (ok, errMsg, statusCode) = await CheckBranchAccessAsync(branchId, requestingUserId);
        if (!ok) return ApiResponse<GarageBundleResponse>.FailureResponse(errMsg, statusCode);

        var validationError = await ValidateBundleItemsAsync(request.Items, branchId);
        if (validationError is not null)
            return ApiResponse<GarageBundleResponse>.FailureResponse(validationError, 422);

        var bundle = request.ToEntity(branchId);
        await _unitOfWork.GarageBundles.AddAsync(bundle);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("CreateGarageBundle: {BundleId} branch={BranchId}", bundle.Id, branchId);

        var result = await _unitOfWork.GarageBundles.GetByIdWithItemsAsync(bundle.Id, ct);
        return ApiResponse<GarageBundleResponse>.CreatedResponse(
            result!.ToResponse(), "Tạo combo thành công");
    }

    public async Task<ApiResponse<GarageBundleResponse>> UpdateBundleAsync(
        Guid id, Guid requestingUserId, UpdateGarageBundleRequest request, CancellationToken ct = default)
    {
        var bundle = await _unitOfWork.GarageBundles.GetByIdWithItemsAsync(id, ct);
        if (bundle is null)
            return ApiResponse<GarageBundleResponse>.NotFoundResponse(
                $"Không tìm thấy combo với id '{id}'.");

        var (ok, errMsg, statusCode) = await CheckBranchAccessAsync(bundle.GarageBranchId, requestingUserId);
        if (!ok) return ApiResponse<GarageBundleResponse>.FailureResponse(errMsg, statusCode);

        var validationError = await ValidateBundleItemsAsync(request.Items, bundle.GarageBranchId);
        if (validationError is not null)
            return ApiResponse<GarageBundleResponse>.FailureResponse(validationError, 422);

        bundle.UpdateFromRequest(request);

        // Soft delete old items, add new ones
        foreach (var item in bundle.Items)
            item.DeletedAt = DateTime.UtcNow;

        var newItems = request.Items.Select((i, idx) => i.ToItemEntity(idx)).ToList();
        newItems.ForEach(i => i.GarageBundleId = bundle.Id);
        bundle.Items.AddRange(newItems);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("UpdateGarageBundle: {BundleId}", id);

        var updated = await _unitOfWork.GarageBundles.GetByIdWithItemsAsync(id, ct);
        return ApiResponse<GarageBundleResponse>.SuccessResponse(
            updated!.ToResponse(), "Cập nhật combo thành công");
    }

    public async Task<ApiResponse<GarageBundleResponse>> UpdateBundleStatusAsync(
        Guid id, Guid requestingUserId, UpdateGarageBundleStatusRequest request, CancellationToken ct = default)
    {
        var bundle = await _unitOfWork.GarageBundles.FindOneAsync(
            b => b.Id == id && b.DeletedAt == null);
        if (bundle is null)
            return ApiResponse<GarageBundleResponse>.NotFoundResponse(
                $"Không tìm thấy combo với id '{id}'.");

        var (ok, errMsg, statusCode) = await CheckBranchAccessAsync(bundle.GarageBranchId, requestingUserId);
        if (!ok) return ApiResponse<GarageBundleResponse>.FailureResponse(errMsg, statusCode);

        bundle.Status = request.Status;
        bundle.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("UpdateGarageBundleStatus: {BundleId} → {Status}", id, request.Status);

        var updated = await _unitOfWork.GarageBundles.GetByIdWithItemsAsync(id, ct);
        return ApiResponse<GarageBundleResponse>.SuccessResponse(
            updated!.ToResponse(), "Cập nhật trạng thái combo thành công");
    }

    public async Task<ApiResponse<bool>> DeleteBundleAsync(
        Guid id, Guid requestingUserId, CancellationToken ct = default)
    {
        var bundle = await _unitOfWork.GarageBundles.FindOneAsync(
            b => b.Id == id && b.DeletedAt == null);
        if (bundle is null)
            return ApiResponse<bool>.NotFoundResponse(
                $"Không tìm thấy combo với id '{id}'.");

        var (ok, errMsg, statusCode) = await CheckBranchAccessAsync(bundle.GarageBranchId, requestingUserId);
        if (!ok) return ApiResponse<bool>.FailureResponse(errMsg, statusCode);

        bundle.DeletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("DeleteGarageBundle: soft deleted {BundleId}", id);

        return ApiResponse<bool>.SuccessResponse(true, "Xóa combo thành công");
    }

    /// <summary>Validate items: exactly one FK per item, items belong to the branch, status Active.</summary>
    private async Task<string?> ValidateBundleItemsAsync(List<BundleItemRequest> items, Guid branchId)
    {
        if (items.Count == 0)
            return "Combo phải có ít nhất một mục.";

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var setCount = (item.ProductId.HasValue ? 1 : 0) + (item.ServiceId.HasValue ? 1 : 0);
            if (setCount != 1)
                return $"Mục #{i + 1}: phải chỉ định đúng một trong ProductId hoặc ServiceId.";

            if (item.ProductId.HasValue)
            {
                var product = await _unitOfWork.GarageProducts.FindOneAsync(
                    p => p.Id == item.ProductId.Value && p.GarageBranchId == branchId && p.DeletedAt == null);
                if (product is null)
                    return $"Mục #{i + 1}: sản phẩm không tồn tại hoặc không thuộc chi nhánh này.";
                if (product.Status != ProductStatus.Active)
                    return $"Mục #{i + 1}: sản phẩm '{product.Name}' không khả dụng.";
            }
            else if (item.ServiceId.HasValue)
            {
                var service = await _unitOfWork.GarageServices.FindOneAsync(
                    s => s.Id == item.ServiceId.Value && s.GarageBranchId == branchId && s.DeletedAt == null);
                if (service is null)
                    return $"Mục #{i + 1}: dịch vụ không tồn tại hoặc không thuộc chi nhánh này.";
                if (service.Status != ProductStatus.Active)
                    return $"Mục #{i + 1}: dịch vụ '{service.Name}' không khả dụng.";
            }
        }

        return null;
    }

    private async Task<(bool Ok, string Message, int StatusCode)> CheckBranchAccessAsync(
        Guid branchId, Guid userId)
    {
        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == branchId && b.DeletedAt == null);
        if (branch is null)
            return (false, $"Không tìm thấy chi nhánh với id '{branchId}'.", 404);

        var garage = await _unitOfWork.Garages.FindOneAsync(
            g => g.Id == branch.GarageId && g.DeletedAt == null);
        if (garage is null)
            return (false, "Không tìm thấy garage.", 404);

        if (garage.OwnerId == userId) return (true, string.Empty, 0);

        var isMember = await _unitOfWork.Members.FindOneAsync(
            m => m.GarageBranchId == branchId
              && m.UserId == userId
              && m.Role == MemberRole.Manager
              && m.DeletedAt == null);

        if (isMember is null)
            return (false, "Bạn không có quyền quản lý combo của chi nhánh này.", 403);

        return (true, string.Empty, 0);
    }
}
