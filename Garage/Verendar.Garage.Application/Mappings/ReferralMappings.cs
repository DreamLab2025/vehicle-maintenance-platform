using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Mappings;

public static class ReferralMappings
{
    public static GarageReferralResponse ToReferralResponse(this GarageReferral referral) =>
        new(referral.ReferredUserId, referral.ReferredAt, referral.ReferralCode);
}
