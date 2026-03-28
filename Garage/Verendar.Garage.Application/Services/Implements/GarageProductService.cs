using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Application.Services.Implements;

public class GarageProductService(
    ILogger<GarageProductService> logger,
    IUnitOfWork unitOfWork) : IGarageProductService
{
    private readonly ILogger<GarageProductService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ApiResponse<List<GarageProductListItemResponse>>> GetProductsByBranchAsync(
        Guid branchId, bool activeOnly, PaginationRequest pagination, CancellationToken ct = default)
    {
        pagination.Normalize();

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == branchId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<List<GarageProductListItemResponse>>.NotFoundResponse(
                string.Format(EndpointMessages.BranchManager.BranchNotFoundByIdFormat, branchId));

        var (items, totalCount) = await _unitOfWork.GarageProducts.GetPagedByBranchIdAsync(
            branchId, activeOnly, pagination.PageNumber, pagination.PageSize, ct);

        return ApiResponse<List<GarageProductListItemResponse>>.SuccessPagedResponse(
            items.Select(p => p.ToListItemResponse()).ToList(),
            totalCount,
            pagination.PageNumber,
            pagination.PageSize,
            EndpointMessages.Product.ListSuccess);
    }

    public async Task<ApiResponse<GarageProductResponse>> GetProductByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var product = await _unitOfWork.GarageProducts.GetByIdWithInstallationAsync(id, ct);
        if (product is null)
            return ApiResponse<GarageProductResponse>.NotFoundResponse(
                string.Format(EndpointMessages.Product.NotFoundByIdFormat, id));

        return ApiResponse<GarageProductResponse>.SuccessResponse(
            product.ToResponse(), EndpointMessages.Product.GetSuccess);
    }

    public async Task<ApiResponse<GarageProductResponse>> CreateProductAsync(
        Guid branchId, Guid requestingUserId, CreateGarageProductRequest request, CancellationToken ct = default)
    {
        var (ok, errMsg, statusCode) = await CheckBranchAccessAsync(branchId, requestingUserId);
        if (!ok) return ApiResponse<GarageProductResponse>.FailureResponse(errMsg, statusCode);

        if (request.InstallationServiceId.HasValue)
        {
            var svc = await _unitOfWork.GarageServices.FindOneAsync(
                s => s.Id == request.InstallationServiceId.Value
                  && s.GarageBranchId == branchId
                  && s.DeletedAt == null);
            if (svc is null)
                return ApiResponse<GarageProductResponse>.FailureResponse(
                    EndpointMessages.Product.InstallationServiceInvalid, 422);
        }

        var product = request.ToEntity(branchId);
        await _unitOfWork.GarageProducts.AddAsync(product);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("CreateGarageProduct: {ProductId} branch={BranchId}", product.Id, branchId);

        var result = await _unitOfWork.GarageProducts.GetByIdWithInstallationAsync(product.Id, ct);
        return ApiResponse<GarageProductResponse>.CreatedResponse(
            result!.ToResponse(), EndpointMessages.Product.CreateSuccess);
    }

    public async Task<ApiResponse<GarageProductResponse>> UpdateProductAsync(
        Guid id, Guid requestingUserId, UpdateGarageProductRequest request, CancellationToken ct = default)
    {
        var product = await _unitOfWork.GarageProducts.GetByIdWithInstallationAsync(id, ct);
        if (product is null)
            return ApiResponse<GarageProductResponse>.NotFoundResponse(
                string.Format(EndpointMessages.Product.NotFoundByIdFormat, id));

        var (ok, errMsg, statusCode) = await CheckBranchAccessAsync(product.GarageBranchId, requestingUserId);
        if (!ok) return ApiResponse<GarageProductResponse>.FailureResponse(errMsg, statusCode);

        if (request.InstallationServiceId.HasValue)
        {
            var svc = await _unitOfWork.GarageServices.FindOneAsync(
                s => s.Id == request.InstallationServiceId.Value
                  && s.GarageBranchId == product.GarageBranchId
                  && s.DeletedAt == null);
            if (svc is null)
                return ApiResponse<GarageProductResponse>.FailureResponse(
                    EndpointMessages.Product.InstallationServiceInvalid, 422);
        }

        product.UpdateFromRequest(request);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("UpdateGarageProduct: {ProductId}", id);

        var updated = await _unitOfWork.GarageProducts.GetByIdWithInstallationAsync(id, ct);
        return ApiResponse<GarageProductResponse>.SuccessResponse(
            updated!.ToResponse(), EndpointMessages.Product.UpdateSuccess);
    }

    public async Task<ApiResponse<GarageProductResponse>> UpdateProductStatusAsync(
        Guid id, Guid requestingUserId, UpdateGarageProductStatusRequest request, CancellationToken ct = default)
    {
        var product = await _unitOfWork.GarageProducts.FindOneAsync(
            p => p.Id == id && p.DeletedAt == null);
        if (product is null)
            return ApiResponse<GarageProductResponse>.NotFoundResponse(
                string.Format(EndpointMessages.Product.NotFoundByIdFormat, id));

        var (ok, errMsg, statusCode) = await CheckBranchAccessAsync(product.GarageBranchId, requestingUserId);
        if (!ok) return ApiResponse<GarageProductResponse>.FailureResponse(errMsg, statusCode);

        product.Status = request.Status;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("UpdateGarageProductStatus: {ProductId} → {Status}", id, request.Status);

        var updated = await _unitOfWork.GarageProducts.GetByIdWithInstallationAsync(id, ct);
        return ApiResponse<GarageProductResponse>.SuccessResponse(
            updated!.ToResponse(), EndpointMessages.Product.UpdateStatusSuccess);
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(
        Guid id, Guid requestingUserId, CancellationToken ct = default)
    {
        var product = await _unitOfWork.GarageProducts.FindOneAsync(
            p => p.Id == id && p.DeletedAt == null);
        if (product is null)
            return ApiResponse<bool>.NotFoundResponse(
                string.Format(EndpointMessages.Product.NotFoundByIdFormat, id));

        var (ok, errMsg, statusCode) = await CheckBranchAccessAsync(product.GarageBranchId, requestingUserId);
        if (!ok) return ApiResponse<bool>.FailureResponse(errMsg, statusCode);

        product.DeletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("DeleteGarageProduct: soft deleted {ProductId}", id);

        return ApiResponse<bool>.SuccessResponse(true, EndpointMessages.Product.DeleteSuccess);
    }

    private async Task<(bool Ok, string Message, int StatusCode)> CheckBranchAccessAsync(
        Guid branchId, Guid userId)
    {
        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == branchId && b.DeletedAt == null);
        if (branch is null)
            return (false, string.Format(EndpointMessages.BranchManager.BranchNotFoundByIdFormat, branchId), 404);

        var garage = await _unitOfWork.Garages.FindOneAsync(
            g => g.Id == branch.GarageId && g.DeletedAt == null);
        if (garage is null)
            return (false, EndpointMessages.Stats.GarageNotFound, 404);

        if (garage.OwnerId == userId) return (true, string.Empty, 0);

        var isMember = await _unitOfWork.Members.FindOneAsync(
            m => m.GarageBranchId == branchId
              && m.UserId == userId
              && m.Role == MemberRole.Manager
              && m.DeletedAt == null);

        if (isMember is null)
            return (false, EndpointMessages.BranchManager.ForbiddenManageProducts, 403);

        return (true, string.Empty, 0);
    }
}
