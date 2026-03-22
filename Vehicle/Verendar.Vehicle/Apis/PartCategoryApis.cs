namespace Verendar.Vehicle.Apis
{
    public static class PartCategoryApis
    {
        public static IEndpointRouteBuilder MapPartCategoryApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/part-categories")
                .MapPartCategoryRoutes()
                .WithTags("Part Category Api")
                .RequireRateLimiting("Fixed");
            return builder;
        }

        public static RouteGroupBuilder MapPartCategoryRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllCategories)
                .WithName("GetAllPartCategories")
                .WithOpenApi(op => { op.Summary = "Lấy danh mục phụ tùng; truyền userVehicleId để lọc theo xe của user"; return op; })
                .RequireAuthorization()
                .Produces<ApiResponse<List<PartCategorySummary>>>(StatusCodes.Status200OK);

            group.MapGet("/{id:guid}", GetCategoryById)
                .WithName("GetPartCategoryById")
                .WithOpenApi(op => { op.Summary = "Get part category by ID"; return op; })
                .RequireAuthorization()
                .Produces<ApiResponse<PartCategoryResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<PartCategoryResponse>>(StatusCodes.Status404NotFound);

            group.MapGet("/{partCategorySlug}/reminders", GetRemindersByCategorySlug)
                .WithName("GetRemindersByCategorySlug")
                .WithOpenApi(op => { op.Summary = "Lấy toàn bộ reminder theo part category code"; return op; })
                .RequireAuthorization()
                .Produces<ApiResponse<List<ReminderDetailDto>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<ReminderDetailDto>>>(StatusCodes.Status404NotFound);

            group.MapPost("/", CreateCategory)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<PartCategoryRequest>())
                .WithName("CreatePartCategory")
                .WithOpenApi(op => { op.Summary = "Create part category (Admin)"; return op; })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<PartCategoryResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<PartCategoryResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<PartCategoryResponse>>(StatusCodes.Status409Conflict);

            group.MapPut("/{id:guid}", UpdateCategory)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<PartCategoryRequest>())
                .WithName("UpdatePartCategory")
                .WithOpenApi(op => { op.Summary = "Update part category (Admin)"; return op; })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<PartCategoryResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<PartCategoryResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<PartCategoryResponse>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<PartCategoryResponse>>(StatusCodes.Status409Conflict);

            group.MapDelete("/{id:guid}", DeleteCategory)
                .WithName("DeletePartCategory")
                .WithOpenApi(op => { op.Summary = "Delete part category (Admin)"; return op; })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound);

            return group;
        }

        private static async Task<IResult> GetAllCategories(
            [AsParameters] PaginationRequest request,
            Guid? userVehicleId,
            ICurrentUserService currentUserService,
            IPartCategoryService service)
        {
            if (userVehicleId.HasValue)
            {
                var userId = currentUserService.UserId;
                if (userId == Guid.Empty)
                    return Results.Unauthorized();

                var filtered = await service.GetCategoriesByVehicleDeclaredPartsAsync(userId, userVehicleId.Value);
                return filtered.ToHttpResult();
            }

            var result = await service.GetAllCategoriesAsync(request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetCategoryById(Guid id, IPartCategoryService service)
        {
            var result = await service.GetCategoryByIdAsync(id);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetRemindersByCategorySlug(
            string partCategorySlug,
            Guid userVehicleId,
            ICurrentUserService currentUserService,
            IPartCategoryService service)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Results.Unauthorized();

            var result = await service.GetRemindersByCategorySlugAsync(userId, userVehicleId, partCategorySlug);
            return result.ToHttpResult();
        }

        private static async Task<IResult> CreateCategory(PartCategoryRequest request, IPartCategoryService service)
        {
            var result = await service.CreateCategoryAsync(request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> UpdateCategory(Guid id, PartCategoryRequest request, IPartCategoryService service)
        {
            var result = await service.UpdateCategoryAsync(id, request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> DeleteCategory(Guid id, IPartCategoryService service)
        {
            var result = await service.DeleteCategoryAsync(id);
            return result.ToHttpResult();
        }
    }
}
