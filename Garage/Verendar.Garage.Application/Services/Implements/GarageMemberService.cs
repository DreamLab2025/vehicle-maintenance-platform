using Verendar.Common.Shared;
using Verendar.Garage.Application.Clients;
using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Mappings;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Application.Services.Implements;

public class GarageMemberService(
    ILogger<GarageMemberService> logger,
    IUnitOfWork unitOfWork,
    IIdentityClient identityClient) : IGarageMemberService
{
    private readonly ILogger<GarageMemberService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IIdentityClient _identityClient = identityClient;

    public async Task<ApiResponse<GarageMemberResponse>> AddMemberAsync(
        Guid garageId,
        Guid requestingUserId,
        AddMemberRequest request,
        CancellationToken ct = default)
    {
        var garage = await _unitOfWork.Garages.FindOneAsync(g => g.Id == garageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<GarageMemberResponse>.NotFoundResponse(EndpointMessages.Member.GarageNotFound);

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == request.BranchId && b.GarageId == garageId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<GarageMemberResponse>.NotFoundResponse(EndpointMessages.Member.BranchNotInGarage);

        var isOwner = garage.OwnerId == requestingUserId;
        if (!isOwner)
        {
            var managerMembership = await _unitOfWork.Members.FindOneAsync(m =>
                m.UserId == requestingUserId
                && m.GarageBranchId == request.BranchId
                && m.Role == MemberRole.Manager
                && m.Status == MemberStatus.Active
                && m.DeletedAt == null);

            if (managerMembership is null)
                return ApiResponse<GarageMemberResponse>.ForbiddenResponse(EndpointMessages.Member.ForbiddenAddMember);

            if (request.Role != MemberRole.Mechanic)
                return ApiResponse<GarageMemberResponse>.ForbiddenResponse(EndpointMessages.Member.ManagerOnlyMechanic);
        }

        var createUserRequest = request.ToCreateMemberUserRequest();
        var userId = request.Role == MemberRole.Manager
            ? await _identityClient.CreateManagerUserAsync(createUserRequest, ct)
            : await _identityClient.CreateMechanicUserAsync(createUserRequest, ct);

        if (!userId.HasValue)
            return ApiResponse<GarageMemberResponse>.FailureResponse(EndpointMessages.Member.IdentityCreateFailed);

        var exists = await _unitOfWork.Members.FindOneAsync(m =>
            m.UserId == userId.Value
            && m.GarageBranchId == request.BranchId
            && m.DeletedAt == null);
        if (exists is not null)
            return ApiResponse<GarageMemberResponse>.ConflictResponse(EndpointMessages.Member.MemberAlreadyInBranch);

        var member = request.ToEntity(userId.Value);

        await _unitOfWork.Members.AddAsync(member);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("AddMember: created {Role} {MemberId} for branch {BranchId}", request.Role, member.Id, request.BranchId);

        return ApiResponse<GarageMemberResponse>.CreatedResponse(member.ToResponse(garageId), EndpointMessages.Member.AddSuccess);
    }

    public async Task<ApiResponse<List<GarageMemberResponse>>> GetMembersAsync(
        Guid garageId,
        Guid branchId,
        Guid requestingUserId,
        PaginationRequest request,
        CancellationToken ct = default)
    {
        request.Normalize();

        var garage = await _unitOfWork.Garages.FindOneAsync(g => g.Id == garageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<List<GarageMemberResponse>>.NotFoundResponse(EndpointMessages.Member.GarageNotFound);

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == branchId && b.GarageId == garageId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<List<GarageMemberResponse>>.NotFoundResponse(EndpointMessages.Member.BranchNotInGarage);

        var isOwner = garage.OwnerId == requestingUserId;
        if (!isOwner)
        {
            var managerMembership = await _unitOfWork.Members.FindOneAsync(m =>
                m.UserId == requestingUserId
                && m.GarageBranchId == branchId
                && m.Role == MemberRole.Manager
                && m.Status == MemberStatus.Active
                && m.DeletedAt == null);

            if (managerMembership is null)
                return ApiResponse<List<GarageMemberResponse>>.ForbiddenResponse(EndpointMessages.Member.ForbiddenListMembers);
        }

        var (items, totalCount) = await _unitOfWork.Members.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            filter: m => m.GarageBranchId == branchId && m.DeletedAt == null,
            orderBy: q => q.OrderByDescending(m => m.CreatedAt));

        return ApiResponse<GarageMemberResponse>.SuccessPagedResponse(
            items.Select(m => m.ToResponse(garageId)).ToList(),
            totalCount,
            request.PageNumber,
            request.PageSize,
            EndpointMessages.Member.ListSuccess);
    }

    public async Task<ApiResponse<GarageMemberResponse>> UpdateMemberStatusAsync(
        Guid memberId,
        Guid requestingUserId,
        UpdateMemberStatusRequest request,
        CancellationToken ct = default)
    {
        var member = await _unitOfWork.Members.FindOneAsync(m => m.Id == memberId && m.DeletedAt == null);
        if (member is null)
            return ApiResponse<GarageMemberResponse>.NotFoundResponse(EndpointMessages.Member.MemberNotFound);

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(b => b.Id == member.GarageBranchId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<GarageMemberResponse>.NotFoundResponse(EndpointMessages.Member.MemberBranchNotFound);

        var garage = await _unitOfWork.Garages.FindOneAsync(g => g.Id == branch.GarageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<GarageMemberResponse>.NotFoundResponse(EndpointMessages.Member.GarageNotFound);

        var isOwner = garage.OwnerId == requestingUserId;
        if (!isOwner)
        {
            var managerMembership = await _unitOfWork.Members.FindOneAsync(m =>
                m.UserId == requestingUserId
                && m.GarageBranchId == member.GarageBranchId
                && m.Role == MemberRole.Manager
                && m.Status == MemberStatus.Active
                && m.DeletedAt == null);

            if (managerMembership is null)
                return ApiResponse<GarageMemberResponse>.ForbiddenResponse(EndpointMessages.Member.ForbiddenUpdateMemberStatus);

            if (member.Role != MemberRole.Mechanic)
                return ApiResponse<GarageMemberResponse>.ForbiddenResponse(EndpointMessages.Member.ManagerOnlyUpdateMechanic);
        }

        member.Status = request.Status;
        member.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse<GarageMemberResponse>.SuccessResponse(
            member.ToResponse(garage.Id),
            EndpointMessages.Member.UpdateStatusSuccess);
    }

    public async Task<ApiResponse<bool>> RemoveMemberAsync(
        Guid memberId,
        Guid requestingUserId,
        CancellationToken ct = default)
    {
        var member = await _unitOfWork.Members.FindOneAsync(m => m.Id == memberId && m.DeletedAt == null);
        if (member is null)
            return ApiResponse<bool>.NotFoundResponse(EndpointMessages.Member.MemberNotFound);

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(b => b.Id == member.GarageBranchId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<bool>.NotFoundResponse(EndpointMessages.Member.MemberBranchNotFound);

        var garage = await _unitOfWork.Garages.FindOneAsync(g => g.Id == branch.GarageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<bool>.NotFoundResponse(EndpointMessages.Member.GarageNotFound);

        var isOwner = garage.OwnerId == requestingUserId;
        if (!isOwner)
        {
            var managerMembership = await _unitOfWork.Members.FindOneAsync(m =>
                m.UserId == requestingUserId
                && m.GarageBranchId == member.GarageBranchId
                && m.Role == MemberRole.Manager
                && m.Status == MemberStatus.Active
                && m.DeletedAt == null);

            if (managerMembership is null)
                return ApiResponse<bool>.ForbiddenResponse(EndpointMessages.Member.ForbiddenRemoveMember);

            if (member.Role != MemberRole.Mechanic)
                return ApiResponse<bool>.ForbiddenResponse(EndpointMessages.Member.ManagerOnlyRemoveMechanic);
        }

        member.DeletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse<bool>.SuccessResponse(true, EndpointMessages.Member.RemoveSuccess);
    }
}
