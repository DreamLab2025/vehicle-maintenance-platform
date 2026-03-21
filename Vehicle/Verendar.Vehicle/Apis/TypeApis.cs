namespace Verendar.Vehicle.Apis
{
    public static class TypeApis
    {
        public static IEndpointRouteBuilder MapTypeApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/types")
                .MapTypeRoutes()
                .WithTags("Type Api")
                .RequireRateLimiting("Fixed");
            return builder;
        }

        public static RouteGroupBuilder MapTypeRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllTypes)
                .WithName("GetAllTypes")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy danh sách tất cả loại xe";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<List<TypeSummary>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id:guid}", GetTypeById)
                .WithName("GetTypeById")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy thông tin loại xe theo ID";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<TypeResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<TypeResponse>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/", CreateVehicleType)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<TypeRequest>())
                .WithName("CreateType")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo mới loại xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<TypeResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<TypeResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<TypeResponse>>(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}", UpdateVehicleType)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<TypeRequest>())
                .WithName("UpdateType")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật loại xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<TypeResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<TypeResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<TypeResponse>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<TypeResponse>>(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id:guid}", DeleteVehicleType)
                .WithName("DeleteType")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xóa loại xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> GetTypeById(Guid id, ITypeService typeService)
        {
            var result = await typeService.GetTypeByIdAsync(id);
            return result.ToHttpResult();
        }

        private static async Task<IResult> DeleteVehicleType(Guid id, ITypeService typeService)
        {
            var result = await typeService.DeleteTypeAsync(id);
            return result.ToHttpResult();
        }

        private static async Task<IResult> UpdateVehicleType(Guid id, TypeRequest request, ITypeService typeService)
        {
            var result = await typeService.UpdateTypeAsync(id, request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> CreateVehicleType(TypeRequest request, ITypeService typeService)
        {
            var result = await typeService.CreateTypeAsync(request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetAllTypes([AsParameters] PaginationRequest paginationRequest, ITypeService typeService)
        {
            var results = await typeService.GetAllTypesAsync(paginationRequest);
            return results.ToHttpResult();
        }
    }
}
