using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using VMP.Common.Shared;
using VMP.Vehicle.Application.Dtos;
using VMP.Vehicle.Application.Services.Interfaces;

namespace VMP.Vehicle.Apis
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

            group.MapPost("/bulk", BulkCreateBrands)
                .WithName("BulkCreateBrands")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo hàng loạt thương hiệu từ JSON (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<BulkBrandResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<BulkBrandResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .DisableAntiforgery();

            group.MapPost("/bulk/upload", BulkCreateBrandsFromFile)
                .WithName("BulkCreateBrandsFromFile")
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<BulkBrandResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<BulkBrandResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .DisableAntiforgery();

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

        private static async Task<IResult> BulkCreateBrands(BulkBrandRequest request, IVehicleBrandService brandService)
        {
            var result = await brandService.BulkCreateBrandsAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> BulkCreateBrandsFromFile([FromForm] IFormFile file, IVehicleBrandService brandService)
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest(ApiResponse<BulkBrandResponse>.FailureResponse("File không được để trống"));
            }

            if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest(ApiResponse<BulkBrandResponse>.FailureResponse("Chỉ chấp nhận file JSON"));
            }

            try
            {
                using var stream = file.OpenReadStream();
                var bulkRequest = await JsonSerializer.DeserializeAsync<BulkBrandRequest>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (bulkRequest == null || bulkRequest.Brands == null || !bulkRequest.Brands.Any())
                {
                    return Results.BadRequest(ApiResponse<BulkBrandResponse>.FailureResponse("File JSON không hợp lệ hoặc rỗng"));
                }

                var result = await brandService.BulkCreateBrandsAsync(bulkRequest);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
            }
            catch (JsonException)
            {
                return Results.BadRequest(ApiResponse<BulkBrandResponse>.FailureResponse("File JSON không đúng định dạng"));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ApiResponse<BulkBrandResponse>.FailureResponse($"Lỗi khi xử lý file: {ex.Message}"));
            }
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
