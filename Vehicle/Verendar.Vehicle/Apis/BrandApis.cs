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
                    operation.Summary = "Lấy danh sách thương hiệu, lọc theo loại xe nếu có typeId";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<BrandSummary>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<BrandSummary>>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id:guid}", GetBrandById)
                .WithName("GetBrandById")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy thông tin thương hiệu theo ID";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<BrandResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<BrandResponse>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/", CreateVehicleBrand)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<BrandRequest>())
                .WithName("CreateBrand")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo mới thương hiệu (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<BrandResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<BrandResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<BrandResponse>>(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}", UpdateVehicleBrand)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<BrandRequest>())
                .WithName("UpdateBrand")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật thương hiệu (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<BrandResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<BrandResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<BrandResponse>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<BrandResponse>>(StatusCodes.Status409Conflict)
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
                .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> GetBrandById(Guid id, IBrandService brandService)
        {
            var result = await brandService.GetBrandByIdAsync(id);
            return result.ToHttpResult();
        }

        private static async Task<IResult> DeleteVehicleBrand(Guid id, IBrandService brandService)
        {
            var result = await brandService.DeleteBrandAsync(id);
            return result.ToHttpResult();
        }

        private static async Task<IResult> UpdateVehicleBrand(Guid id, BrandRequest request, IBrandService brandService)
        {
            var result = await brandService.UpdateBrandAsync(id, request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> CreateVehicleBrand(BrandRequest request, IBrandService brandService)
        {
            var result = await brandService.CreateBrandAsync(request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetAllBrands([AsParameters] BrandFilterRequest request, IBrandService brandService)
        {
            if (request.TypeId.HasValue)
            {
                var filtered = await brandService.GetBrandsByTypeIdAsync(request.TypeId.Value);
                return filtered.ToHttpResult();
            }

            var results = await brandService.GetAllBrandsAsync(request);
            return results.ToHttpResult();
        }
    }
}
