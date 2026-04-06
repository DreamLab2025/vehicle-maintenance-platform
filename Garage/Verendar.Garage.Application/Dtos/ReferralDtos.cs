using Verendar.Common.Shared;

namespace Verendar.Garage.Application.Dtos;

public class ReferralListRequest : PaginationRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public record GarageReferralResponse(
    Guid ReferredUserId,
    DateTime ReferredAt,
    string ReferralCode);

public record ReferralStatsResponse(
    int TotalReferrals,
    string? ReferralCode,
    string? ReferralLink,
    string? QrCodeUrl);
