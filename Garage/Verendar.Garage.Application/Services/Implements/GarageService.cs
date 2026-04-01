using MassTransit;
using Verendar.Common.Shared;
using Verendar.Garage.Application.Clients;
using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Services.Interfaces;
using Verendar.Garage.Contracts.Events;
using GarageEntity = Verendar.Garage.Domain.Entities.Garage;

namespace Verendar.Garage.Application.Services.Implements;

public class GarageService(
    ILogger<GarageService> logger,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    IIdentityClient identityClient) : IGarageService
{
    private readonly ILogger<GarageService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IIdentityClient _identityClient = identityClient;

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
            EndpointMessages.OwnerGarage.ListSuccess);
    }

    public async Task<ApiResponse<GarageDetailResponse>> GetMyGarageAsync(Guid ownerId, CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.GetWithBranchesAsync(g => g.OwnerId == ownerId, ct);

        if (garage is null)
            return ApiResponse<GarageDetailResponse>.NotFoundResponse(EndpointMessages.OwnerGarage.MyGarageNotRegistered);

        return ApiResponse<GarageDetailResponse>.SuccessResponse(
            garage.ToDetailResponse(), EndpointMessages.OwnerGarage.GetDetailSuccess);
    }

    public async Task<ApiResponse<GarageDetailResponse>> GetGarageByIdAsync(Guid garageId, CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.GetWithBranchesAsync(g => g.Id == garageId, ct);

        if (garage is null)
            return ApiResponse<GarageDetailResponse>.NotFoundResponse(EndpointMessages.OwnerGarage.GarageNotFoundPlain);

        return ApiResponse<GarageDetailResponse>.SuccessResponse(
            garage.ToDetailResponse(), EndpointMessages.OwnerGarage.GetDetailSuccess);
    }

    public async Task<ApiResponse<GarageDetailResponse>> GetGarageBySlugAsync(string slug, CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.GetWithBranchesAsync(g => g.Slug == slug, ct);

        if (garage is null)
            return ApiResponse<GarageDetailResponse>.NotFoundResponse(
                string.Format(EndpointMessages.OwnerGarage.GarageNotFoundBySlugFormat, slug));

        return ApiResponse<GarageDetailResponse>.SuccessResponse(
            garage.ToDetailResponse(), EndpointMessages.OwnerGarage.GetDetailSuccess);
    }

    public async Task<ApiResponse<GarageResponse>> UpdateGarageStatusAsync(
        Guid garageId, UpdateGarageStatusRequest request, Guid adminUserId, CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.FindOneAsync(
            g => g.Id == garageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<GarageResponse>.NotFoundResponse(
                string.Format(EndpointMessages.OwnerGarage.GarageNotFoundByIdFormat, garageId));

        if (!IsValidAdminTransition(garage.Status, request.Status))
            return ApiResponse<GarageResponse>.FailureResponse(
                string.Format(EndpointMessages.OwnerGarage.StatusTransitionInvalidFormat, garage.Status, request.Status), 400);

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

        await HandleRoleAndMembersAsync(garage, request.Status, ct);

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

        return ApiResponse<GarageResponse>.SuccessResponse(garage.ToResponse(), EndpointMessages.OwnerGarage.UpdateStatusSuccess);
    }

    public async Task<ApiResponse<GarageResponse>> UpdateGarageInfoAsync(
        Guid garageId, Guid ownerId, GarageRequest request, CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.FindOneAsync(
            g => g.Id == garageId && g.OwnerId == ownerId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<GarageResponse>.NotFoundResponse(EndpointMessages.OwnerGarage.GarageNotFoundPlain);

        if (garage.Status == GarageStatus.Active || garage.Status == GarageStatus.Suspended)
            return ApiResponse<GarageResponse>.FailureResponse(
                EndpointMessages.OwnerGarage.CannotEditWhenActiveOrSuspended, 400);

        garage.UpdateFromRequest(request);
        garage.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("UpdateGarageInfo: garage {GarageId} updated by owner {OwnerId}", garageId, ownerId);

        return ApiResponse<GarageResponse>.SuccessResponse(garage.ToResponse(), EndpointMessages.OwnerGarage.UpdateInfoSuccess);
    }

    public async Task<ApiResponse<GarageResponse>> ResubmitGarageAsync(
        Guid garageId, Guid ownerId, CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.FindOneAsync(
            g => g.Id == garageId && g.OwnerId == ownerId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<GarageResponse>.NotFoundResponse(EndpointMessages.OwnerGarage.GarageNotFoundPlain);

        if (garage.Status != GarageStatus.Rejected)
            return ApiResponse<GarageResponse>.FailureResponse(
                EndpointMessages.OwnerGarage.ResubmitOnlyWhenRejected, 400);

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

        return ApiResponse<GarageResponse>.SuccessResponse(garage.ToResponse(), EndpointMessages.OwnerGarage.ResubmitSuccess);
    }

    private async Task HandleRoleAndMembersAsync(GarageEntity garage, GarageStatus newStatus, CancellationToken ct)
    {
        try
        {
            if (newStatus == GarageStatus.Active)
            {
                var assigned = await _identityClient.AssignRoleAsync(garage.OwnerId, "GarageOwner", ct);
                if (!assigned)
                    _logger.LogWarning("HandleRoleAndMembers: failed to assign GarageOwner role for owner {OwnerId}", garage.OwnerId);
                return;
            }

            if (newStatus == GarageStatus.Suspended || newStatus == GarageStatus.Rejected)
            {
                var revoked = await _identityClient.RevokeRoleAsync(garage.OwnerId, "GarageOwner", ct);
                if (!revoked)
                    _logger.LogWarning("HandleRoleAndMembers: failed to revoke GarageOwner role for owner {OwnerId}", garage.OwnerId);

                var activeMembers = await _unitOfWork.Members.GetActiveByGarageIdAsync(garage.Id, ct);
                if (activeMembers.Count == 0) return;

                foreach (var member in activeMembers)
                {
                    member.Status = MemberStatus.Inactive;
                    member.UpdatedAt = DateTime.UtcNow;
                }
                await _unitOfWork.SaveChangesAsync(ct);

                var memberUserIds = activeMembers.Select(m => m.UserId);
                var deactivated = await _identityClient.BulkDeactivateAsync(memberUserIds, ct);
                if (!deactivated)
                    _logger.LogWarning("HandleRoleAndMembers: failed to deactivate {Count} member accounts for garage {GarageId}",
                        activeMembers.Count, garage.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "HandleRoleAndMembers: unexpected error for garage {GarageId} status {Status}",
                garage.Id, newStatus);
        }
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
                    EndpointMessages.OwnerGarage.ConflictExistingGarage);
            }

            // Garage bị Rejected — hướng dẫn dùng flow edit + resubmit thay vì tạo mới
            return ApiResponse<GarageResponse>.FailureResponse(
                EndpointMessages.OwnerGarage.RejectedUseEditResubmitFlow, 400);
        }

        var garage = request.ToEntity(ownerId);

        garage.Slug = await SlugUtils.EnsureUniqueAsync(
            SlugUtils.ToSlug(request.BusinessName, 110),
            async s => (await _unitOfWork.Garages.FindOneAsync(g => g.Slug == s)) != null,
            maxLength: 110);

        await _unitOfWork.Garages.AddAsync(garage);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("CreateGarage: created garage {GarageId} for owner {OwnerId}", garage.Id, ownerId);

        return ApiResponse<GarageResponse>.CreatedResponse(garage.ToResponse(), EndpointMessages.OwnerGarage.RegisterSuccess);
    }
}
