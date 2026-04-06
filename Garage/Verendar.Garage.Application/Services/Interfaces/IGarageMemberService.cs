using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IGarageMemberService
{
    Task<ApiResponse<GarageMemberResponse>> AddMemberAsync(
        Guid garageId,
        Guid requestingUserId,
        AddMemberRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<List<GarageMemberResponse>>> GetMembersAsync(
        GarageMemberQueryRequest query,
        Guid requestingUserId,
        CancellationToken ct = default);

    Task<ApiResponse<GarageMemberResponse>> UpdateMemberStatusAsync(
        Guid memberId,
        Guid requestingUserId,
        UpdateMemberStatusRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<bool>> RemoveMemberAsync(
        Guid memberId,
        Guid requestingUserId,
        CancellationToken ct = default);

    Task<ApiResponse<MemberPasswordResponse>> GetMemberPasswordAsync(
        Guid memberId,
        Guid garageId,
        Guid callerId,
        CancellationToken ct = default);
}
