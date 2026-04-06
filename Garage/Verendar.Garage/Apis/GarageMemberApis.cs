using Microsoft.AspNetCore.Mvc;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Apis;

public static class GarageMemberApis
{
    public static IEndpointRouteBuilder MapGarageMemberApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/members")
            .MapGarageMemberRoutes()
            .WithTags("Garage Member Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    public static RouteGroupBuilder MapGarageMemberRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/", AddMember)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<AddMemberRequest>())
            .WithName("AddGarageMember")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Owner hoặc Manager thêm thành viên cho chi nhánh";
                return operation;
            })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<GarageMemberResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<GarageMemberResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageMemberResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageMemberResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/", GetMembers)
            .WithName("GetGarageMembers")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Owner hoặc Manager xem danh sách thành viên theo branch (phân trang)";
                return operation;
            })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<List<GarageMemberResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<GarageMemberResponse>>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<List<GarageMemberResponse>>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/{id:guid}/status", UpdateMemberStatus)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateMemberStatusRequest>())
            .WithName("UpdateGarageMemberStatus")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Owner hoặc Manager cập nhật trạng thái thành viên";
                return operation;
            })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<GarageMemberResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageMemberResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageMemberResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{id:guid}", RemoveMember)
            .WithName("RemoveGarageMember")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Owner hoặc Manager xóa mềm thành viên";
                return operation;
            })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> AddMember(
        [FromQuery] Guid garageId,
        [FromBody] AddMemberRequest request,
        ICurrentUserService currentUserService,
        IGarageMemberService memberService,
        CancellationToken ct)
    {
        var result = await memberService.AddMemberAsync(garageId, currentUserService.UserId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetMembers(
        [AsParameters] GarageMemberQueryRequest query,
        ICurrentUserService currentUserService,
        IGarageMemberService memberService,
        CancellationToken ct)
    {
        var result = await memberService.GetMembersAsync(query, currentUserService.UserId, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateMemberStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateMemberStatusRequest request,
        ICurrentUserService currentUserService,
        IGarageMemberService memberService,
        CancellationToken ct)
    {
        var result = await memberService.UpdateMemberStatusAsync(id, currentUserService.UserId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> RemoveMember(
        [FromRoute] Guid id,
        ICurrentUserService currentUserService,
        IGarageMemberService memberService,
        CancellationToken ct)
    {
        var result = await memberService.RemoveMemberAsync(id, currentUserService.UserId, ct);
        return result.ToHttpResult();
    }
}
