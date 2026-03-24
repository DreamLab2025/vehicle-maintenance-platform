using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.ExternalServices;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Application.Services.Implements;

public class GarageBranchService(
    ILogger<GarageBranchService> logger,
    IUnitOfWork unitOfWork,
    IGeocodingService geocodingService) : IGarageBranchService
{
    private readonly ILogger<GarageBranchService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IGeocodingService _geocodingService = geocodingService;

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
        var coords = await _geocodingService.GeocodeAsync(geocodeQuery, ct);
        if (coords.HasValue)
        {
            branch.Latitude = coords.Value.Latitude;
            branch.Longitude = coords.Value.Longitude;
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

        return ApiResponse<GarageBranchResponse>.CreatedResponse(
            branch.ToResponse(), "Tạo chi nhánh thành công");
    }
}
