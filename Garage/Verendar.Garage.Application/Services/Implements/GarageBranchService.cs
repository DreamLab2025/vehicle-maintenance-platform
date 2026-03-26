using Verendar.Garage.Application.Clients;
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
                $"Không tìm thấy garage với id '{garageId}'.");

        if (garage.OwnerId != requestingUserId)
            return ApiResponse<GarageBranchResponse>.ForbiddenResponse(
                "Bạn không có quyền thêm chi nhánh cho garage này.");

        var branch = request.ToEntity(garageId);

        // Generate unique slug from branch name
        branch.Slug = await SlugUtils.EnsureUniqueAsync(
            SlugUtils.ToSlug(request.Name, 120),
            async s => (await _unitOfWork.GarageBranches.FindOneAsync(b => b.Slug == s)) != null,
            maxLength: 120,
            cancellationToken: ct);

        // Geocode address — soft failure (branch still saved with 0,0 if geocoding fails)
        var geocodeQuery = string.IsNullOrWhiteSpace(request.Address.HouseNumber)
            ? $"{request.Address.StreetDetail}, Việt Nam"
            : $"{request.Address.HouseNumber} {request.Address.StreetDetail}, Việt Nam";
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
            response, "Tạo chi nhánh thành công");
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
                $"Không tìm thấy chi nhánh với id '{branchId}'.");

        var response = branch.ToResponse();

        if (branch.Latitude != 0 || branch.Longitude != 0)
            response.MapLinks = await _locationClient.GetMapLinksAsync(branch.Latitude, branch.Longitude, ct);

        return ApiResponse<GarageBranchResponse>.SuccessResponse(
            response, "Lấy thông tin chi nhánh thành công");
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
                    new List<BranchMapItemResponse>(), 0, request.PageNumber, request.PageSize, "Không tìm thấy địa chỉ");
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

        return ApiResponse<List<BranchMapItemResponse>>.SuccessPagedResponse(
            items.Select(b => b.ToBranchMapItemResponse()).ToList(),
            totalCount,
            request.PageNumber,
            request.PageSize,
            "Lấy danh sách chi nhánh thành công");
    }
}
