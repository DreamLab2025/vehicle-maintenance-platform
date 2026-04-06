using Microsoft.AspNetCore.Mvc;

namespace Verendar.Vehicle.Apis
{
    public static class VariantApis
    {
        public static IEndpointRouteBuilder MapVariantApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/variants")
                .MapVariantRoutes()
                .WithTags("Variant Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapVariantRoutes(this RouteGroupBuilder group)
        {
            group.MapPost("/", CreateVariant)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<VariantRequest>())
                .WithName("CreateVariant")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Tạo hình ảnh/màu mới cho mẫu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}", UpdateVariant)
                .AddEndpointFilter(ValidationEndpointFilter.Validate<VariantUpdateRequest>())
                .WithName("UpdateVariant")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Cập nhật hình ảnh/màu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status400BadRequest)
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<VariantResponse>>(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id:guid}", DeleteVariant)
                .WithName("DeleteVariant")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xóa hình ảnh/màu xe (Admin)";
                    return operation;
                })
                .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Admin)))
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> CreateVariant([FromBody] VariantRequest request, IVariantService service)
        {
            var result = await service.CreateImageAsync(request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> UpdateVariant(Guid id, [FromBody] VariantUpdateRequest request, IVariantService service)
        {
            var result = await service.UpdateImageAsync(id, request);
            return result.ToHttpResult();
        }

        private static async Task<IResult> DeleteVariant(Guid id, IVariantService service)
        {
            var result = await service.DeleteImageAsync(id);
            return result.ToHttpResult();
        }
    }
}
