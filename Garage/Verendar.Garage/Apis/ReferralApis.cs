using System.Security.Claims;
using Verendar.Common.Jwt;
using Verendar.Garage.Application.Constants;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Apis;

public static class ReferralApis
{
    public static IEndpointRouteBuilder MapReferralApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/garages/{garageId:guid}/referrals")
            .MapReferralRoutes()
            .WithTags("Referral Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    private static RouteGroupBuilder MapReferralRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetReferrals)
            .WithName("GetGarageReferrals")
            .WithOpenApi(op => { op.Summary = "Danh sách khách được garage giới thiệu (chủ garage hoặc Admin)"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.Admin)))
            .Produces<ApiResponse<List<GarageReferralResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<GarageReferralResponse>>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<List<GarageReferralResponse>>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/stats", GetReferralStats)
            .WithName("GetGarageReferralStats")
            .WithOpenApi(op => { op.Summary = "Thống kê giới thiệu + link + QR code (chủ garage hoặc Admin)"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.Admin)))
            .Produces<ApiResponse<ReferralStatsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ReferralStatsResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<ReferralStatsResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    // ─── Handlers ─────────────────────────────────────────────────────────────

    private static async Task<IResult> GetReferrals(
        Guid garageId,
        [AsParameters] ReferralListRequest request,
        IReferralService referralService,
        ICurrentUserService currentUser,
        ClaimsPrincipal principal,
        CancellationToken ct)
    {
        var isAdmin = principal.IsInRole(nameof(RoleType.Admin));
        var result = await referralService.GetReferralsAsync(garageId, currentUser.UserId, isAdmin, request, ct);
        return Results.Json(result, statusCode: result.StatusCode);
    }

    private static async Task<IResult> GetReferralStats(
        Guid garageId,
        IReferralService referralService,
        ICurrentUserService currentUser,
        ClaimsPrincipal principal,
        CancellationToken ct)
    {
        var isAdmin = principal.IsInRole(nameof(RoleType.Admin));
        var result = await referralService.GetReferralStatsAsync(garageId, currentUser.UserId, isAdmin, ct);
        return Results.Json(result, statusCode: result.StatusCode);
    }
}
