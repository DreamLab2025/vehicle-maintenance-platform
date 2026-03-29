using Verendar.Garage.Application.Clients;
using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Application.Services.Implements;

public class GarageBranchService(
    ILogger<GarageBranchService> logger,
    IUnitOfWork unitOfWork,
    ILocationClient locationClient) : IGarageBranchService
{
    private readonly ILogger<GarageBranchService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILocationClient _locationClient = locationClient;

    public async Task<ApiResponse<GarageBranchResponse>> CreateBranchAsync(
        Guid garageId,
        Guid requestingUserId,
        GarageBranchRequest request,
        CancellationToken ct = default)
    {
        // Ownership check
        var garage = await _unitOfWork.Garages.FindOneAsync(
            g => g.Id == garageId && g.DeletedAt == null);

        if (garage is null)
            return ApiResponse<GarageBranchResponse>.NotFoundResponse(
                string.Format(EndpointMessages.GarageBranches.GarageNotFoundByIdFormat, garageId));

        if (garage.OwnerId != requestingUserId)
            return ApiResponse<GarageBranchResponse>.ForbiddenResponse(
                EndpointMessages.GarageBranches.ForbiddenAddBranch);

        if (garage.Status != GarageStatus.Active)
            return ApiResponse<GarageBranchResponse>.FailureResponse(
                EndpointMessages.GarageBranches.GarageNotApprovedAddBranch, 422);

        // Validate province + ward via Location service
        var (isValid, provinceName, wardName) = await _locationClient.ValidateLocationAsync(
            request.Address.ProvinceCode, request.Address.WardCode, ct);
        if (!isValid)
            return ApiResponse<GarageBranchResponse>.FailureResponse(
                EndpointMessages.GarageBranches.InvalidProvinceWard, 422);

        var branch = request.ToEntity(garageId);

        // Generate unique slug from branch name
        branch.Slug = await SlugUtils.EnsureUniqueAsync(
            SlugUtils.ToSlug(request.Name, 120),
            async s => (await _unitOfWork.GarageBranches.FindOneAsync(b => b.Slug == s)) != null,
            maxLength: 120,
            cancellationToken: ct);

        // Assemble full address for geocoding using ward/province names from Location service
        var geocodeQuery = $"{request.Address.StreetDetail}, {wardName}, {provinceName}";
        var coords = await _locationClient.GeocodeAsync(geocodeQuery, ct);
        MapLinksDto? mapLinks = null;
        if (coords.HasValue)
        {
            branch.Latitude = coords.Value.Latitude;
            branch.Longitude = coords.Value.Longitude;
            mapLinks = await _locationClient.GetMapLinksAsync(coords.Value.Latitude, coords.Value.Longitude, ct);
        }
        else
        {
            _logger.LogWarning("CreateBranch: geocoding failed for '{Address}', branch saved without coordinates",
                geocodeQuery);
        }

        await _unitOfWork.GarageBranches.AddAsync(branch);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("CreateBranch: created branch {BranchId} for garage {GarageId}",
            branch.Id, garageId);

        var response = branch.ToResponse();
        response.MapLinks = mapLinks;

        return ApiResponse<GarageBranchResponse>.CreatedResponse(
            response, EndpointMessages.GarageBranches.CreateSuccess);
    }

    public async Task<ApiResponse<GarageBranchResponse>> GetBranchByIdAsync(
        Guid garageId,
        Guid branchId,
        CancellationToken ct = default)
    {
        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == branchId && b.GarageId == garageId && b.DeletedAt == null);

        if (branch is null)
            return ApiResponse<GarageBranchResponse>.NotFoundResponse(
                string.Format(EndpointMessages.BranchManager.BranchNotFoundByIdFormat, branchId));

        var response = await ToBranchDetailResponseAsync(branch, ct);

        return ApiResponse<GarageBranchResponse>.SuccessResponse(
            response, EndpointMessages.GarageBranches.GetDetailSuccess);
    }

    public async Task<ApiResponse<GarageBranchResponse>> GetMyBranchAsync(Guid userId, CancellationToken ct = default)
    {
        var member = await _unitOfWork.Members.GetLatestActiveMembershipWithBranchAsync(userId, ct);
        if (member is null)
            return ApiResponse<GarageBranchResponse>.NotFoundResponse(EndpointMessages.GarageBranches.MyBranchNoMembership);

        var branch = member.GarageBranch;
        var response = await ToBranchDetailResponseAsync(branch, ct);

        return ApiResponse<GarageBranchResponse>.SuccessResponse(
            response, EndpointMessages.GarageBranches.GetDetailSuccess);
    }

    public async Task<ApiResponse<List<GarageBranchSummaryResponse>>> GetBranchesAsync(
        Guid garageId,
        PaginationRequest request,
        CancellationToken ct = default)
    {
        request.Normalize();

        var garageExists = await _unitOfWork.Garages.FindOneAsync(
            g => g.Id == garageId && g.DeletedAt == null);
        if (garageExists is null)
            return ApiResponse<List<GarageBranchSummaryResponse>>.NotFoundResponse(
                string.Format(EndpointMessages.GarageBranches.GarageNotFoundByIdFormat, garageId));

        var (items, totalCount) = await _unitOfWork.GarageBranches.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            filter: b => b.GarageId == garageId && b.DeletedAt == null,
            orderBy: q => q.OrderByDescending(b => b.CreatedAt));

        return ApiResponse<GarageBranchSummaryResponse>.SuccessPagedResponse(
            items.Select(b => b.ToSummaryResponse()).ToList(),
            totalCount,
            request.PageNumber,
            request.PageSize,
            EndpointMessages.GarageBranches.ListSuccess);
    }

    public async Task<ApiResponse<GarageBranchResponse>> UpdateBranchAsync(
        Guid garageId,
        Guid branchId,
        Guid requestingUserId,
        GarageBranchRequest request,
        CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.FindOneAsync(
            g => g.Id == garageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<GarageBranchResponse>.NotFoundResponse(
                string.Format(EndpointMessages.GarageBranches.GarageNotFoundByIdFormat, garageId));

        if (garage.OwnerId != requestingUserId)
            return ApiResponse<GarageBranchResponse>.ForbiddenResponse(
                EndpointMessages.GarageBranches.ForbiddenUpdateBranch);

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == branchId && b.GarageId == garageId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<GarageBranchResponse>.NotFoundResponse(
                string.Format(EndpointMessages.BranchManager.BranchNotFoundByIdFormat, branchId));

        bool addressChanged =
            branch.Address.ProvinceCode != request.Address.ProvinceCode ||
            branch.Address.WardCode != request.Address.WardCode ||
            branch.Address.StreetDetail != request.Address.StreetDetail;

        MapLinksDto? mapLinks = null;

        if (addressChanged)
        {
            var (isValid, provinceName, wardName) = await _locationClient.ValidateLocationAsync(
                request.Address.ProvinceCode, request.Address.WardCode, ct);
            if (!isValid)
                return ApiResponse<GarageBranchResponse>.FailureResponse(
                    EndpointMessages.GarageBranches.InvalidProvinceWard, 422);

            var geocodeQuery = $"{request.Address.StreetDetail}, {wardName}, {provinceName}";
            var coords = await _locationClient.GeocodeAsync(geocodeQuery, ct);
            if (coords.HasValue)
            {
                branch.Latitude = coords.Value.Latitude;
                branch.Longitude = coords.Value.Longitude;
                mapLinks = await _locationClient.GetMapLinksAsync(coords.Value.Latitude, coords.Value.Longitude, ct);
            }
            else
            {
                _logger.LogWarning("UpdateBranch: geocoding failed for '{Address}', keeping old coordinates", geocodeQuery);
            }
        }
        else if (branch.Latitude != 0 || branch.Longitude != 0)
        {
            mapLinks = await _locationClient.GetMapLinksAsync(branch.Latitude, branch.Longitude, ct);
        }

        branch.UpdateFromRequest(request);
        branch.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("UpdateBranch: updated branch {BranchId} for garage {GarageId}", branchId, garageId);

        var response = branch.ToResponse();
        response.MapLinks = mapLinks;
        return ApiResponse<GarageBranchResponse>.SuccessResponse(response, EndpointMessages.GarageBranches.UpdateSuccess);
    }

    public async Task<ApiResponse<bool>> DeleteBranchAsync(
        Guid garageId,
        Guid branchId,
        Guid requestingUserId,
        CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.FindOneAsync(
            g => g.Id == garageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<bool>.NotFoundResponse(
                string.Format(EndpointMessages.GarageBranches.GarageNotFoundByIdFormat, garageId));

        if (garage.OwnerId != requestingUserId)
            return ApiResponse<bool>.ForbiddenResponse(
                EndpointMessages.GarageBranches.ForbiddenDeleteBranch);

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == branchId && b.GarageId == garageId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<bool>.NotFoundResponse(
                string.Format(EndpointMessages.BranchManager.BranchNotFoundByIdFormat, branchId));

        branch.DeletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("DeleteBranch: soft deleted branch {BranchId} for garage {GarageId}", branchId, garageId);

        return ApiResponse<bool>.SuccessResponse(true, EndpointMessages.GarageBranches.DeleteSuccess);
    }

    public async Task<ApiResponse<GarageBranchResponse>> UpdateBranchStatusAsync(
        Guid garageId,
        Guid branchId,
        Guid requestingUserId,
        UpdateBranchStatusRequest request,
        CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.FindOneAsync(
            g => g.Id == garageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<GarageBranchResponse>.NotFoundResponse(
                string.Format(EndpointMessages.GarageBranches.GarageNotFoundByIdFormat, garageId));

        if (garage.OwnerId != requestingUserId)
            return ApiResponse<GarageBranchResponse>.ForbiddenResponse(
                EndpointMessages.GarageBranches.ForbiddenManageBranchStatus);

        if (garage.Status != GarageStatus.Active)
            return ApiResponse<GarageBranchResponse>.FailureResponse(
                EndpointMessages.GarageBranches.GarageNotApprovedBranchStatus, 422);

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == branchId && b.GarageId == garageId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<GarageBranchResponse>.NotFoundResponse(
                string.Format(EndpointMessages.BranchManager.BranchNotFoundByIdFormat, branchId));

        branch.Status = request.Status;
        branch.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("UpdateBranchStatus: branch {BranchId} → {Status}", branchId, request.Status);

        var response = branch.ToResponse();
        if (branch.Latitude != 0 || branch.Longitude != 0)
            response.MapLinks = await _locationClient.GetMapLinksAsync(branch.Latitude, branch.Longitude, ct);

        return ApiResponse<GarageBranchResponse>.SuccessResponse(response, EndpointMessages.GarageBranches.UpdateStatusSuccess);
    }

    public async Task<ApiResponse<List<BranchMapItemResponse>>> GetBranchesForMapAsync(
        BranchMapSearchRequest request,
        CancellationToken ct = default)
    {
        request.Normalize();

        double? centerLat = request.Lat;
        double? centerLng = request.Lng;

        // Geocode text address if lat/lng not provided
        if (centerLat is null && centerLng is null && !string.IsNullOrWhiteSpace(request.Address))
        {
            var coords = await _locationClient.GeocodeAsync(request.Address, ct);
            if (coords.HasValue)
            {
                centerLat = coords.Value.Latitude;
                centerLng = coords.Value.Longitude;
            }
            else
            {
                _logger.LogWarning("GetBranchesForMap: geocoding failed for address '{Address}'", request.Address);
                return ApiResponse<List<BranchMapItemResponse>>.SuccessPagedResponse(
                    new List<BranchMapItemResponse>(), 0, request.PageNumber, request.PageSize, EndpointMessages.GarageBranches.GeocodeAddressNotFound);
            }
        }

        // Compute bounding box from center + radius
        double? minLat = null, maxLat = null, minLng = null, maxLng = null;
        if (centerLat.HasValue && centerLng.HasValue)
        {
            var radiusKm = Math.Clamp(request.RadiusKm, 1, 200);
            var latDelta = radiusKm / 111.0;
            var lngDelta = radiusKm / (111.0 * Math.Cos(centerLat.Value * Math.PI / 180.0));
            minLat = centerLat.Value - latDelta;
            maxLat = centerLat.Value + latDelta;
            minLng = centerLng.Value - lngDelta;
            maxLng = centerLng.Value + lngDelta;
        }

        var (items, totalCount) = await _unitOfWork.GarageBranches.GetBranchesForMapAsync(
            request.PageNumber,
            request.PageSize,
            minLat, maxLat, minLng, maxLng,
            ct);

        var ratingSummary = await _unitOfWork.Reviews.GetBulkRatingSummaryAsync(
            items.Select(b => b.Id), ct);

        var mapped = items.Select(b =>
        {
            var r = b.ToBranchMapItemResponse();
            if (ratingSummary.TryGetValue(b.Id, out var rating))
            {
                r.AverageRating = Math.Round(rating.AverageRating, 1);
                r.ReviewCount = rating.ReviewCount;
            }
            return r;
        }).ToList();

        return ApiResponse<List<BranchMapItemResponse>>.SuccessPagedResponse(
            mapped,
            totalCount,
            request.PageNumber,
            request.PageSize,
            EndpointMessages.GarageBranches.ListSuccess);
    }

    private async Task<GarageBranchResponse> ToBranchDetailResponseAsync(GarageBranch branch, CancellationToken ct)
    {
        var response = branch.ToResponse();

        if (branch.Latitude != 0 || branch.Longitude != 0)
            response.MapLinks = await _locationClient.GetMapLinksAsync(branch.Latitude, branch.Longitude, ct);

        var (avg, count) = await _unitOfWork.Reviews.GetRatingSummaryAsync(branch.Id, ct);
        response.AverageRating = count > 0 ? Math.Round(avg, 1) : null;
        response.ReviewCount = count;

        return response;
    }
}
