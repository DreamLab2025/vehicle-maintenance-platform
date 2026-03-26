using Verendar.Common.Shared;
using Verendar.Garage.Application.Clients;
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
            return ApiResponse<GarageMemberResponse>.NotFoundResponse("Không tìm thấy garage.");

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == request.BranchId && b.GarageId == garageId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<GarageMemberResponse>.NotFoundResponse("Không tìm thấy chi nhánh của garage.");

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
                return ApiResponse<GarageMemberResponse>.ForbiddenResponse("Bạn không có quyền thêm thành viên.");

            if (request.Role != MemberRole.Mechanic)
                return ApiResponse<GarageMemberResponse>.ForbiddenResponse("Manager chỉ được phép thêm Mechanic.");
        }

        var createUserRequest = request.ToCreateMemberUserRequest();
        var userId = request.Role == MemberRole.Manager
            ? await _identityClient.CreateManagerUserAsync(createUserRequest, ct)
            : await _identityClient.CreateMechanicUserAsync(createUserRequest, ct);

        if (!userId.HasValue)
            return ApiResponse<GarageMemberResponse>.FailureResponse("Không thể tạo tài khoản thành viên từ Identity service.");

        var exists = await _unitOfWork.Members.FindOneAsync(m =>
            m.UserId == userId.Value
            && m.GarageBranchId == request.BranchId
            && m.DeletedAt == null);
        if (exists is not null)
            return ApiResponse<GarageMemberResponse>.ConflictResponse("Thành viên đã tồn tại trong chi nhánh.");

        var member = request.ToEntity(userId.Value);

        await _unitOfWork.Members.AddAsync(member);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("AddMember: created {Role} {MemberId} for branch {BranchId}", request.Role, member.Id, request.BranchId);

        return ApiResponse<GarageMemberResponse>.CreatedResponse(member.ToResponse(garageId), "Thêm thành viên thành công");
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
            return ApiResponse<List<GarageMemberResponse>>.NotFoundResponse("Không tìm thấy garage.");

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(
            b => b.Id == branchId && b.GarageId == garageId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<List<GarageMemberResponse>>.NotFoundResponse("Không tìm thấy chi nhánh của garage.");

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
                return ApiResponse<List<GarageMemberResponse>>.ForbiddenResponse("Bạn không có quyền xem danh sách thành viên.");
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
            "Lấy danh sách thành viên thành công");
    }

    public async Task<ApiResponse<GarageMemberResponse>> UpdateMemberStatusAsync(
        Guid memberId,
        Guid requestingUserId,
        UpdateMemberStatusRequest request,
        CancellationToken ct = default)
    {
        var member = await _unitOfWork.Members.FindOneAsync(m => m.Id == memberId && m.DeletedAt == null);
        if (member is null)
            return ApiResponse<GarageMemberResponse>.NotFoundResponse("Không tìm thấy thành viên.");

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(b => b.Id == member.GarageBranchId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<GarageMemberResponse>.NotFoundResponse("Không tìm thấy chi nhánh của thành viên.");

        var garage = await _unitOfWork.Garages.FindOneAsync(g => g.Id == branch.GarageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<GarageMemberResponse>.NotFoundResponse("Không tìm thấy garage.");

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
                return ApiResponse<GarageMemberResponse>.ForbiddenResponse("Bạn không có quyền cập nhật trạng thái thành viên.");

            if (member.Role != MemberRole.Mechanic)
                return ApiResponse<GarageMemberResponse>.ForbiddenResponse("Manager chỉ được phép cập nhật Mechanic trong chi nhánh của mình.");
        }

        member.Status = request.Status;
        member.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse<GarageMemberResponse>.SuccessResponse(
            member.ToResponse(garage.Id),
            "Cập nhật trạng thái thành viên thành công");
    }

    public async Task<ApiResponse<bool>> RemoveMemberAsync(
        Guid memberId,
        Guid requestingUserId,
        CancellationToken ct = default)
    {
        var member = await _unitOfWork.Members.FindOneAsync(m => m.Id == memberId && m.DeletedAt == null);
        if (member is null)
            return ApiResponse<bool>.NotFoundResponse("Không tìm thấy thành viên.");

        var branch = await _unitOfWork.GarageBranches.FindOneAsync(b => b.Id == member.GarageBranchId && b.DeletedAt == null);
        if (branch is null)
            return ApiResponse<bool>.NotFoundResponse("Không tìm thấy chi nhánh của thành viên.");

        var garage = await _unitOfWork.Garages.FindOneAsync(g => g.Id == branch.GarageId && g.DeletedAt == null);
        if (garage is null)
            return ApiResponse<bool>.NotFoundResponse("Không tìm thấy garage.");

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
                return ApiResponse<bool>.ForbiddenResponse("Bạn không có quyền xóa thành viên.");

            if (member.Role != MemberRole.Mechanic)
                return ApiResponse<bool>.ForbiddenResponse("Manager chỉ được phép xóa Mechanic trong chi nhánh của mình.");
        }

        member.DeletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse<bool>.SuccessResponse(true, "Xóa thành viên thành công");
    }
}
