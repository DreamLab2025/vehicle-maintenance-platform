using Microsoft.AspNetCore.Mvc;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Apis;

public static class GarageBundleApis
{
    public static IEndpointRouteBuilder MapGarageBundleApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/garage-bundles")
            .MapGarageBundleRoutes()
            .WithTags("Garage Bundle Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    public static RouteGroupBuilder MapGarageBundleRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetByBranch)
            .WithName("GetGarageBundles")
            .WithOpenApi(op => { op.Summary = "Danh sách combo theo chi nhánh"; return op; })
            .Produces<ApiResponse<List<GarageBundleListItemResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<GarageBundleListItemResponse>>>(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetGarageBundleById")
            .WithOpenApi(op => { op.Summary = "Chi tiết combo kèm danh sách items"; return op; })
            .Produces<ApiResponse<GarageBundleResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageBundleResponse>>(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateGarageBundleRequest>())
            .WithName("CreateGarageBundle")
            .WithOpenApi(op => { op.Summary = "Owner/Manager tạo combo cho chi nhánh"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<GarageBundleResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<GarageBundleResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageBundleResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageBundleResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateGarageBundleRequest>())
            .WithName("UpdateGarageBundle")
            .WithOpenApi(op => { op.Summary = "Owner/Manager cập nhật combo"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<GarageBundleResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageBundleResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageBundleResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageBundleResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/{id:guid}/status", UpdateStatus)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateGarageBundleStatusRequest>())
            .WithName("UpdateGarageBundleStatus")
            .WithOpenApi(op => { op.Summary = "Owner/Manager bật/tắt trạng thái combo"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<GarageBundleResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageBundleResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageBundleResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteGarageBundle")
            .WithOpenApi(op => { op.Summary = "Owner/Manager xóa combo (soft delete)"; return op; })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GetByBranch(
        [AsParameters] GarageBundleQueryRequest query,
        IGarageBundleService bundleService,
        CancellationToken ct)
    {
        var result = await bundleService.GetBundlesByBranchAsync(query, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetById(
        [FromRoute] Guid id,
        IGarageBundleService bundleService,
        CancellationToken ct)
    {
        var result = await bundleService.GetBundleByIdAsync(id, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Create(
        [FromQuery] Guid branchId,
        [FromBody] CreateGarageBundleRequest request,
        ICurrentUserService currentUserService,
        IGarageBundleService bundleService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await bundleService.CreateBundleAsync(branchId, userId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateGarageBundleRequest request,
        ICurrentUserService currentUserService,
        IGarageBundleService bundleService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await bundleService.UpdateBundleAsync(id, userId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateGarageBundleStatusRequest request,
        ICurrentUserService currentUserService,
        IGarageBundleService bundleService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await bundleService.UpdateBundleStatusAsync(id, userId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Delete(
        [FromRoute] Guid id,
        ICurrentUserService currentUserService,
        IGarageBundleService bundleService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await bundleService.DeleteBundleAsync(id, userId, ct);
        return result.ToHttpResult();
    }
}
