using Verendar.Common.Jwt;
using Verendar.Common.Shared;
using Verendar.Media.Application.Dtos;
using Verendar.Media.Application.Services.Interfaces;

namespace Verendar.Media.Apis
{
    public static class MediaFileApis
    {
        public static IEndpointRouteBuilder MapMediaFileApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("api/v1/media-files")
                .WithTags("Media Files")
                .MapMediaFileRoutes()
                .RequireRateLimiting("Fixed")
                .RequireAuthorization();

            return builder;
        }

        public static RouteGroupBuilder MapMediaFileRoutes(this RouteGroupBuilder group)
        {
            MapInitUploadRoute(group, "/init-upload/avatar", "Avatar", "Lấy Presigned URL upload avatar", "avatar");
            MapInitUploadRoute(group, "/init-upload/vehicle-types", "InitUploadVehicleTypes", "Lấy Presigned URL upload ảnh loại xe", "vehicle-types");
            MapInitUploadRoute(group, "/init-upload/vehicle-brands", "InitUploadVehicleBrands", "Lấy Presigned URL upload ảnh hãng xe", "vehicle-brands");
            MapInitUploadRoute(group, "/init-upload/vehicle-variants", "InitUploadVehicleVariants", "Lấy Presigned URL upload ảnh phiên bản xe (màu)", "vehicle-variants");
            MapInitUploadRoute(group, "/init-upload/part-categories", "InitUploadPartCategories", "Lấy Presigned URL upload icon danh mục phụ tùng", "part-categories");
            MapInitUploadRoute(group, "/init-upload/misc", "InitUploadMisc", "Lấy Presigned URL upload file khác", "misc");

            group.MapPut("{id:guid}/confirm", ConfirmUploadFile)
                .WithName("Confirm Upload File")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xác nhận upload file thành công";
                    return operation;
                })
                .RequireAuthorization()
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> ConfirmUploadFile(
            Guid id,
            ICurrentUserService currentUserService,
            IMediaUploadService mediaUploadService)
        {
            var result = await mediaUploadService.ConfirmUploadFileAsync(id, currentUserService.UserId);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        }

        private static void MapInitUploadRoute(RouteGroupBuilder group, string pattern, string name, string summary, string folderKey)
        {
            group.MapPost(pattern, (InitUploadRequest request, IMediaUploadService service, ICurrentUserService currentUser)
                => InitiateUpload(request, service, currentUser, folderKey))
                .WithName(name)
                .WithOpenApi(operation => { operation.Summary = summary; return operation; })
                .RequireAuthorization()
                .Produces<ApiResponse<InitUploadResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<InitUploadResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);
        }

        private static async Task<IResult> InitiateUpload(
            InitUploadRequest request,
            IMediaUploadService mediaUploadService,
            ICurrentUserService currentUser,
            string folderKey)
        {
            var result = await mediaUploadService.InitiateUploadAsync(request, currentUser.UserId, folderKey);

            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        }
    }
}
