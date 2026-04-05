using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Application.Services.Implements;

public class GarageServiceService(
    ILogger<GarageServiceService> logger,
    IUnitOfWork unitOfWork) : IGarageServiceService
{
    private readonly ILogger<GarageServiceService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ApiResponse<List<GarageServiceListItemResponse>>> GetServicesByBranchAsync(
        Guid branchId, bool activeOnly, PaginationRequest pagination, CancellationToken ct = default)
    {
        pagination.Normalize();

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == branchId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<List<GarageServiceListItemResponse>>.NotFoundResponse(
                string.Format(EndpointMessages.BranchManager.BranchNotFoundByIdFormat, branchId));

        var (items, totalCount) = await _unitOfWork.GarageServices.GetPagedByBranchIdAsync(
            branchId, activeOnly, pagination.PageNumber, pagination.PageSize, ct);

        return ApiResponse<List<GarageServiceListItemResponse>>.SuccessPagedResponse(
            items.Select(s => s.ToListItemResponse()).ToList(),
            totalCount,
            pagination.PageNumber,
            pagination.PageSize,
            EndpointMessages.OfferedServices.ListSuccess);
    }

    public async Task<ApiResponse<GarageServiceResponse>> GetServiceByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var service = await _unitOfWork.GarageServices.GetByIdWithCategoryAsync(id, ct);
        if (service is null)
            return ApiResponse<GarageServiceResponse>.NotFoundResponse(
                string.Format(EndpointMessages.OfferedServices.NotFoundByIdFormat, id));

        return ApiResponse<GarageServiceResponse>.SuccessResponse(
            service.ToResponse(), EndpointMessages.OfferedServices.GetSuccess);
    }

    public async Task<ApiResponse<GarageServiceResponse>> CreateServiceAsync(
        Guid branchId, Guid requestingUserId, CreateGarageServiceRequest request, CancellationToken ct = default)
    {
        var (ok, errMsg, statusCode) = await CheckBranchAccessAsync(branchId, requestingUserId);
        if (!ok) return ApiResponse<GarageServiceResponse>.FailureResponse(errMsg, statusCode);

        if (request.ServiceCategoryId.HasValue)
        {
            var cat = await _unitOfWork.ServiceCategories.FindOneAsync(
                c => c.Id == request.ServiceCategoryId.Value && c.DeletedAt == null);
            if (cat is null)
                return ApiResponse<GarageServiceResponse>.FailureResponse(
                    EndpointMessages.OfferedServices.CategoryNotFound, 422);
        }

        var service = request.ToEntity(branchId);
        await _unitOfWork.GarageServices.AddAsync(service);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("CreateGarageService: {ServiceId} branch={BranchId}", service.Id, branchId);

        var result = await _unitOfWork.GarageServices.GetByIdWithCategoryAsync(service.Id, ct);
        return ApiResponse<GarageServiceResponse>.CreatedResponse(
            result!.ToResponse(), EndpointMessages.OfferedServices.CreateSuccess);
    }

    public async Task<ApiResponse<GarageServiceResponse>> UpdateServiceAsync(
        Guid id, Guid requestingUserId, UpdateGarageServiceRequest request, CancellationToken ct = default)
    {
        var service = await _unitOfWork.GarageServices.GetByIdWithCategoryForUpdateAsync(id, ct);
        if (service is null)
            return ApiResponse<GarageServiceResponse>.NotFoundResponse(
                string.Format(EndpointMessages.OfferedServices.NotFoundByIdFormat, id));

        var (ok, errMsg, statusCode) = await CheckBranchAccessAsync(service.GarageBranchId, requestingUserId);
        if (!ok) return ApiResponse<GarageServiceResponse>.FailureResponse(errMsg, statusCode);

        if (request.ServiceCategoryId.HasValue)
        {
            var cat = await _unitOfWork.ServiceCategories.FindOneAsync(
                c => c.Id == request.ServiceCategoryId.Value && c.DeletedAt == null);
            if (cat is null)
                return ApiResponse<GarageServiceResponse>.FailureResponse(
                    EndpointMessages.OfferedServices.CategoryNotFound, 422);
        }

        service.UpdateFromRequest(request);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("UpdateGarageService: {ServiceId}", id);

        var updated = await _unitOfWork.GarageServices.GetByIdWithCategoryAsync(id, ct);
        return ApiResponse<GarageServiceResponse>.SuccessResponse(
            updated!.ToResponse(), EndpointMessages.OfferedServices.UpdateSuccess);
    }

    public async Task<ApiResponse<GarageServiceResponse>> UpdateServiceStatusAsync(
        Guid id, Guid requestingUserId, UpdateGarageServiceStatusRequest request, CancellationToken ct = default)
    {
        var service = await _unitOfWork.GarageServices.FindOneAsync(
            s => s.Id == id && s.DeletedAt == null);
        if (service is null)
            return ApiResponse<GarageServiceResponse>.NotFoundResponse(
                string.Format(EndpointMessages.OfferedServices.NotFoundByIdFormat, id));

        var (ok, errMsg, statusCode) = await CheckBranchAccessAsync(service.GarageBranchId, requestingUserId);
        if (!ok) return ApiResponse<GarageServiceResponse>.FailureResponse(errMsg, statusCode);

        service.Status = request.Status;
        service.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("UpdateGarageServiceStatus: {ServiceId} → {Status}", id, request.Status);

        var updated = await _unitOfWork.GarageServices.GetByIdWithCategoryAsync(id, ct);
        return ApiResponse<GarageServiceResponse>.SuccessResponse(
            updated!.ToResponse(), EndpointMessages.OfferedServices.UpdateStatusSuccess);
    }

    public async Task<ApiResponse<bool>> DeleteServiceAsync(
        Guid id, Guid requestingUserId, CancellationToken ct = default)
    {
        var service = await _unitOfWork.GarageServices.FindOneAsync(
            s => s.Id == id && s.DeletedAt == null);
        if (service is null)
            return ApiResponse<bool>.NotFoundResponse(
                string.Format(EndpointMessages.OfferedServices.NotFoundByIdFormat, id));

        var (ok, errMsg, statusCode) = await CheckBranchAccessAsync(service.GarageBranchId, requestingUserId);
        if (!ok) return ApiResponse<bool>.FailureResponse(errMsg, statusCode);

        service.DeletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("DeleteGarageService: soft deleted {ServiceId}", id);

        return ApiResponse<bool>.SuccessResponse(true, EndpointMessages.OfferedServices.DeleteSuccess);
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
            return (false, EndpointMessages.BranchManager.ForbiddenManageServices, 403);

        return (true, string.Empty, 0);
    }
}
