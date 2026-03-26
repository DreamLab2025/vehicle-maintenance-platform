using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Apis;

public static class GarageBranchApis
{
    public static IEndpointRouteBuilder MapGarageBranchApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/garages/{garageId:guid}/branches")
            .MapGarageBranchRoutes()
            .WithTags("Garage Branch Api")
            .RequireRateLimiting("Fixed");

        builder.MapGroup("/api/v1/garages/branches")
            .MapGarageBranchMapRoutes()
            .WithTags("Garage Branch Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    public static RouteGroupBuilder MapGarageBranchRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/{branchId:guid}", GetBranchById)
            .WithName("GetGarageBranchById")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Xem chi tiết chi nhánh garage";
                return operation;
            })
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateBranch)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<GarageBranchRequest>())
            .WithName("CreateGarageBranch")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Tạo chi nhánh mới cho garage (chỉ chủ garage)";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<GarageBranchResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    public static RouteGroupBuilder MapGarageBranchMapRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/maps", SearchBranchesOnMap)
            .WithName("SearchGarageBranchesOnMap")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Tìm kiếm chi nhánh trên bản đồ (theo địa chỉ hoặc tọa độ + bán kính)";
                return operation;
            })
            .Produces<ApiResponse<List<BranchMapItemResponse>>>(StatusCodes.Status200OK);

        return group;
    }

    private static async Task<IResult> SearchBranchesOnMap(
        [AsParameters] BranchMapSearchRequest request,
        IGarageBranchService branchService,
        CancellationToken ct)
    {
        var result = await branchService.GetBranchesForMapAsync(request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetBranchById(
        Guid garageId,
        Guid branchId,
        IGarageBranchService branchService,
        CancellationToken ct)
    {
        var result = await branchService.GetBranchByIdAsync(garageId, branchId, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateBranch(
        Guid garageId,
        GarageBranchRequest request,
        ICurrentUserService currentUserService,
        IGarageBranchService branchService,
        CancellationToken ct)
    {
        var result = await branchService.CreateBranchAsync(
            garageId,
            currentUserService.UserId,
            request,
            ct);
        return result.ToHttpResult();
    }
}
