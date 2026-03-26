using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Mappings;

public static class GarageMemberMappings
{
    public static CreateMemberUserRequest ToCreateMemberUserRequest(this AddMemberRequest request) =>
        new(request.FullName, request.Email, request.PhoneNumber);

    public static GarageMember ToEntity(this AddMemberRequest request, Guid userId) =>
        new()
        {
            UserId = userId,
            GarageBranchId = request.BranchId,
            Role = request.Role,
            Status = MemberStatus.Active,
            DisplayName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

    public static GarageMemberResponse ToResponse(this GarageMember member, Guid garageId)
    {
        return new GarageMemberResponse
        {
            Id = member.Id,
            UserId = member.UserId,
            BranchId = member.GarageBranchId,
            GarageId = garageId,
            Role = member.Role,
            Status = member.Status,
            FullName = member.DisplayName,
            Email = member.Email,
            PhoneNumber = member.PhoneNumber
        };
    }
}
