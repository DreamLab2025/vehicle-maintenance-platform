using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Application.Services.Implements;

public class ReferralService(
    ILogger<ReferralService> logger,
    IUnitOfWork unitOfWork) : IReferralService
{
    private readonly ILogger<ReferralService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private const string AppBaseUrl = "https://app.verendar.vn";
    private const string QrApiUrl = "https://api.qrserver.com/v1/create-qr-code/?data={0}&size=300x300";

    public async Task<ApiResponse<List<GarageReferralResponse>>> GetReferralsAsync(
        Guid garageId,
        Guid actorId,
        bool isAdmin,
        ReferralListRequest request,
        CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.FindOneAsync(g => g.Id == garageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<List<GarageReferralResponse>>.NotFoundResponse(EndpointMessages.Referral.GarageNotFound);

        if (!isAdmin && garage.OwnerId != actorId)
            return ApiResponse<List<GarageReferralResponse>>.ForbiddenResponse(EndpointMessages.Referral.Forbidden);

        request.Normalize();

        var (items, total) = await _unitOfWork.Referrals.GetPagedByGarageAsync(
            garageId,
            request.PageNumber,
            request.PageSize,
            request.DateFrom,
            request.DateTo,
            ct);

        return ApiResponse<List<GarageReferralResponse>>.SuccessPagedResponse(
            items.Select(r => r.ToReferralResponse()).ToList(),
            total,
            request.PageNumber,
            request.PageSize,
            EndpointMessages.Referral.ListSuccess);
    }

    public async Task<ApiResponse<ReferralStatsResponse>> GetReferralStatsAsync(
        Guid garageId,
        Guid actorId,
        bool isAdmin,
        CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.FindOneAsync(g => g.Id == garageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<ReferralStatsResponse>.NotFoundResponse(EndpointMessages.Referral.GarageNotFound);

        if (!isAdmin && garage.OwnerId != actorId)
            return ApiResponse<ReferralStatsResponse>.ForbiddenResponse(EndpointMessages.Referral.Forbidden);

        var total = await _unitOfWork.Referrals.CountByGarageAsync(garageId, ct);

        string? referralLink = null;
        string? qrCodeUrl = null;

        if (garage.ReferralCode is not null)
        {
            referralLink = $"{AppBaseUrl}/register?ref={garage.ReferralCode}";
            qrCodeUrl = string.Format(QrApiUrl, Uri.EscapeDataString(referralLink));
        }

        return ApiResponse<ReferralStatsResponse>.SuccessResponse(
            new ReferralStatsResponse(total, garage.ReferralCode, referralLink, qrCodeUrl),
            EndpointMessages.Referral.StatsSuccess);
    }
}
