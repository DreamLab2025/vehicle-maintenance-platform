namespace Verendar.Vehicle.Apis
{
    public static class PartProductApis
    {
        public static IEndpointRouteBuilder MapPartProductApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/part-products")
                .MapPartProductRoutes()
                .WithTags("Part Product Api")
                .RequireRateLimiting("Fixed");
            return builder;
        }

        public static RouteGroupBuilder MapPartProductRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/category/{categoryId:guid}", GetProductsByCategory)
                .WithName("GetProductsByCategory")
                .WithOpenApi(op => { op.Summary = "Get products by category (paginated)"; return op; })
                .RequireAuthorization()
                .Produces<ApiResponse<List<PartProductResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<PartProductResponse>>>(StatusCodes.Status404NotFound);

            group.MapGet("/{id:guid}", GetProductById)
                .WithName("GetPartProductById")
                .WithOpenApi(op => { op.Summary = "Get part product by ID"; return op; })
                .RequireAuthorization()
                .Produces<ApiResponse<PartProductResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<PartProductResponse>>(StatusCodes.Status404NotFound);

            group.MapPost("/", CreateProduct)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<PartProductRequest>())
                .WithName("CreatePartProduct")
                .WithOpenApi(op => { op.Summary = "Create part product (Admin)"; return op; })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<PartProductResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<PartProductResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<PartProductResponse>>(StatusCodes.Status404NotFound);

            group.MapPut("/{id:guid}", UpdateProduct)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<PartProductRequest>())
                .WithName("UpdatePartProduct")
                .WithOpenApi(op => { op.Summary = "Update part product (Admin)"; return op; })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<PartProductResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<PartProductResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<PartProductResponse>>(StatusCodes.Status404NotFound);

            group.MapDelete("/{id:guid}", DeleteProduct)
                .WithName("DeletePartProduct")
                .WithOpenApi(op => { op.Summary = "Delete part product (Admin)"; return op; })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound);

            return group;
        }

        private static async Task<IResult> GetProductsByCategory(Guid categoryId, [AsParameters] PaginationRequest paginationRequest, IPartProductService service)
        {
            var result = await service.GetProductsByCategoryAsync(categoryId, paginationRequest);
            return result.ToHttpResult();
        }

        private static async Task<IResult> GetProductById(Guid id, IPartProductService service)
        {
            var result = await service.GetProductByIdAsync(id);
            return result.ToHttpResult();
        }

        private static async Task<IResult> CreateProduct(PartProductRequest request, IPartProductService service)
        {
            var result = await service.CreateProductAsync(request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> UpdateProduct(Guid id, PartProductRequest request, IPartProductService service)
        {
            var result = await service.UpdateProductAsync(id, request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> DeleteProduct(Guid id, IPartProductService service)
        {
            var result = await service.DeleteProductAsync(id);
            return result.ToHttpResult();
        }
    }
}
