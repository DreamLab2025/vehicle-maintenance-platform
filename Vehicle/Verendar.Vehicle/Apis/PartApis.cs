using Verendar.Common.Jwt;
using Verendar.Common.Shared;
using Verendar.Vehicle.Application.Dtos;
using Verendar.Vehicle.Application.Services.Interfaces;
using Verendar.Common.EndpointFilters;

namespace Verendar.Vehicle.Apis
{
    public static class PartApis
    {
        public static IEndpointRouteBuilder MapPartApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/parts")
                .MapPartRoutes()
                .WithTags("Part Api")
                .RequireRateLimiting("Fixed");
            return builder;
        }

        public static RouteGroupBuilder MapPartRoutes(this RouteGroupBuilder group)
        {
            // Part Category Routes
            group.MapGet("/categories", GetAllCategories)
                .WithName("GetAllPartCategories")
                .WithOpenApi(op => { op.Summary = "Get all part categories"; return op; })
                .RequireAuthorization()
                .Produces<ApiResponse<List<PartCategoryResponse>>>(StatusCodes.Status200OK);

            group.MapGet("/categories/{id:guid}", GetCategoryById)
                .WithName("GetPartCategoryById")
                .WithOpenApi(op => { op.Summary = "Get part category by ID"; return op; })
                .RequireAuthorization()
                .Produces<ApiResponse<PartCategoryResponse>>(StatusCodes.Status200OK);

            group.MapGet("/categories/user-vehicle/{vehicleId:guid}", GetCategoriesByVehicleDeclaredParts)
                .WithName("GetPartCategoriesByUserVehicleDeclaredParts")
                .WithOpenApi(op => { op.Summary = "Lấy danh mục phụ tùng đã khai báo theo xe của user"; return op; })
                .RequireAuthorization()
                .Produces<ApiResponse<List<PartCategoryResponse>>>(StatusCodes.Status200OK);

            group.MapGet("/categories/{partCategoryCode}/reminders/user-vehicle/{userVehicleId:guid}", GetRemindersByCategoryCode)
                .WithName("GetRemindersByCategoryCode")
                .WithOpenApi(op => { op.Summary = "Lấy toàn bộ reminder (current + lịch sử) theo part category code"; return op; })
                .RequireAuthorization()
                .Produces<ApiResponse<List<ReminderWithPartCategoryDto>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<ReminderWithPartCategoryDto>>>(StatusCodes.Status404NotFound);

            group.MapPost("/categories", CreateCategory)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<PartCategoryRequest>())
                .WithName("CreatePartCategory")
                .WithOpenApi(op => { op.Summary = "Create part category (Admin)"; return op; })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<PartCategoryResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<PartCategoryResponse>>(StatusCodes.Status400BadRequest);

            group.MapPut("/categories/{id:guid}", UpdateCategory)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<PartCategoryRequest>())
                .WithName("UpdatePartCategory")
                .WithOpenApi(op => { op.Summary = "Update part category (Admin)"; return op; })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<PartCategoryResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<PartCategoryResponse>>(StatusCodes.Status400BadRequest);

            group.MapDelete("/categories/{id:guid}", DeleteCategory)
                .WithName("DeletePartCategory")
                .WithOpenApi(op => { op.Summary = "Delete part category (Admin)"; return op; })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest);

            // Part Product Routes
            group.MapGet("/products/category/{categoryId:guid}", GetProductsByCategory)
                .WithName("GetProductsByCategory")
                .WithOpenApi(op => { op.Summary = "Get products by category"; return op; })
                .RequireAuthorization()
                .Produces<ApiResponse<List<PartProductResponse>>>(StatusCodes.Status200OK);

            group.MapGet("/products/{id:guid}", GetProductById)
                .WithName("GetPartProductById")
                .WithOpenApi(op => { op.Summary = "Get part product by ID"; return op; })
                .RequireAuthorization()
                .Produces<ApiResponse<PartProductResponse>>(StatusCodes.Status200OK);

            group.MapPost("/products", CreateProduct)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<PartProductRequest>())
                .WithName("CreatePartProduct")
                .WithOpenApi(op => { op.Summary = "Create part product (Admin)"; return op; })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<PartProductResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<PartProductResponse>>(StatusCodes.Status400BadRequest);

            group.MapPut("/products/{id:guid}", UpdateProduct)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<PartProductRequest>())
                .WithName("UpdatePartProduct")
                .WithOpenApi(op => { op.Summary = "Update part product (Admin)"; return op; })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<PartProductResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<PartProductResponse>>(StatusCodes.Status400BadRequest);

            group.MapDelete("/products/{id:guid}", DeleteProduct)
                .WithName("DeletePartProduct")
                .WithOpenApi(op => { op.Summary = "Delete part product (Admin)"; return op; })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest);

            return group;
        }

        // Part Category Handlers
        private static async Task<IResult> GetAllCategories([AsParameters] PaginationRequest request, IPartCategoryService service)
        {
            var result = await service.GetAllCategoriesAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> GetCategoryById(Guid id, IPartCategoryService service)
        {
            var result = await service.GetCategoryByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> GetCategoriesByVehicleDeclaredParts(Guid vehicleId, IPartCategoryService service)
        {
            var result = await service.GetCategoriesByVehicleDeclaredPartsAsync(vehicleId);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> GetRemindersByCategoryCode(
            string partCategoryCode,
            Guid userVehicleId,
            ICurrentUserService currentUserService,
            IPartCategoryService service)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var result = await service.GetRemindersByCategoryCodeAsync(userId, userVehicleId, partCategoryCode);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> CreateCategory(PartCategoryRequest request, IPartCategoryService service)
        {
            var result = await service.CreateCategoryAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> UpdateCategory(Guid id, PartCategoryRequest request, IPartCategoryService service)
        {
            var result = await service.UpdateCategoryAsync(id, request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> DeleteCategory(Guid id, IPartCategoryService service)
        {
            var result = await service.DeleteCategoryAsync(id);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        // Part Product Handlers
        private static async Task<IResult> GetProductsByCategory(Guid categoryId, IPartProductService service)
        {
            var result = await service.GetProductsByCategoryAsync(categoryId);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> GetProductById(Guid id, IPartProductService service)
        {
            var result = await service.GetProductByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> CreateProduct(PartProductRequest request, IPartProductService service)
        {
            var result = await service.CreateProductAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> UpdateProduct(Guid id, PartProductRequest request, IPartProductService service)
        {
            var result = await service.UpdateProductAsync(id, request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> DeleteProduct(Guid id, IPartProductService service)
        {
            var result = await service.DeleteProductAsync(id);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
    }
}
