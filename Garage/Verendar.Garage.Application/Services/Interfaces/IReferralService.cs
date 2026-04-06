using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IReferralService
{
    Task<ApiResponse<List<GarageReferralResponse>>> GetReferralsAsync(
        Guid garageId,
        Guid actorId,
        bool isAdmin,
        ReferralListRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<ReferralStatsResponse>> GetReferralStatsAsync(
        Guid garageId,
        Guid actorId,
        bool isAdmin,
        CancellationToken ct = default);
}
