using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Services.Interfaces;

namespace Verendar.Vehicle.Apis
{
    public static class BrandApis
    {
        public static IEndpointRouteBuilder MapBrandApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/brands")
                .MapBrandRoutes()
                .WithTags("Brand Api")
                .RequireRateLimiting("Fixed");
            return builder;
        }

        public static RouteGroupBuilder MapBrandRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllBrands)
                .WithName("GetAllBrands")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách tất cả thương hiệu";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<BrandResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<BrandResponse>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/types/{typeId:guid}", GetBrandsByType)
                .WithName("GetBrandsByType")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách thương hiệu theo loại xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<BrandResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<BrandResponse>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/", CreateVehicleBrand)
                .WithName("CreateBrand")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo mới thương hiệu (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<BrandResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<BrandResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}", UpdateVehicleBrand)
                .WithName("UpdateBrand")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật thương hiệu (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<BrandResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<BrandResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id:guid}", DeleteVehicleBrand)
                .WithName("DeleteBrand")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xóa thương hiệu (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> DeleteVehicleBrand(Guid id, IVehicleBrandService brandService)
        {
            var result = await brandService.DeleteBrandAsync(id);
            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }
            return Results.BadRequest(result);
        }

        private static async Task<IResult> UpdateVehicleBrand(Guid id, BrandRequest request, IVehicleBrandService brandService)
        {
            var result = await brandService.UpdateBrandAsync(id, request);
            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }
            return Results.BadRequest(result);
        }

        private static async Task<IResult> CreateVehicleBrand(BrandRequest request, IVehicleBrandService brandService)
        {
            var result = await brandService.CreateBrandAsync(request);
            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }
            return Results.BadRequest(result);
        }

        private static async Task<IResult> GetAllBrands([AsParameters] PaginationRequest paginationRequest, IVehicleBrandService brandService)
        {
            var results = await brandService.GetAllBrandsAsync(paginationRequest);
            if (results.IsSuccess)
            {
                return Results.Ok(results);
            }
            return Results.NotFound(results);
        }

        private static async Task<IResult> GetBrandsByType(Guid typeId, IVehicleBrandService brandService)
        {
            var results = await brandService.GetBrandsByTypeIdAsync(typeId);
            if (results.IsSuccess)
            {
                return Results.Ok(results);
            }
            return Results.NotFound(results);
        }
    }
}
