namespace Verendar.Notification.Apis
{
    public static class NotificationApis
    {
        public static IEndpointRouteBuilder MapNotificationApi(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/api/v1/notifications")
                .WithTags("Notification Api")
                .RequireAuthorization();

            group.MapGet("/", GetAllNotificationsForUser)
                .WithName("GetAllNotificationsForUser")
                .WithOpenApi(operation =>
                {
                    operation.Summary =
                        "Lấy danh sách thông báo.";
                    return operation;
                })
                .Produces<ApiResponse<List<NotificationListItemDto>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/status", GetNotificationStatus)
                .WithName("GetNotificationStatus")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Lấy trạng thái đọc: isRead, unReadCount";
                    return operation;
                })
                .Produces<ApiResponse<NotificationStatusDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/read-all", MarkAllNotificationsAsRead)
                .WithName("MarkAllNotificationsAsRead")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Đánh dấu tất cả thông báo là đã đọc";
                    return operation;
                })
                .Produces<ApiResponse<int>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/{id:guid}/read", MarkOneNotificationAsRead)
                .WithName("MarkOneNotificationAsRead")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Đánh dấu một thông báo là đã đọc";
                    return operation;
                })
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id:guid}", GetNotificationDetailForUser)
                .WithName("GetNotificationDetailForUser")
                .WithOpenApi(operation =>
                {
                    operation.Summary =
                        "Lấy chi tiết thông báo.";
                    return operation;
                })
                .Produces<ApiResponse<NotificationListItemDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<NotificationListItemDto>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id:guid}", SoftDeleteNotificationById)
                .WithName("SoftDeleteNotificationById")
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Xóa mềm thông báo theo id (chỉ của user hiện tại)";
                    return operation;
                })
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return builder;
        }

        private static async Task<IResult> GetAllNotificationsForUser(
            ICurrentUserService currentUser,
            INotificationService notificationService,
            [AsParameters] PaginationRequest request,
            CancellationToken cancellationToken = default)
        {
            var response = await notificationService.GetInAppNotificationsForUserAsync(
                currentUser.UserId,
                request,
                cancellationToken);
            return response.ToHttpResult();
        }

        private static async Task<IResult> GetNotificationDetailForUser(
            Guid id,
            ICurrentUserService currentUser,
            INotificationService notificationService,
            CancellationToken cancellationToken = default)
        {
            var response = await notificationService.GetNotificationDetailForUserAsync(
                currentUser.UserId,
                id,
                cancellationToken);
            return response.ToHttpResult();
        }

        private static async Task<IResult> GetNotificationStatus(
            ICurrentUserService currentUser,
            INotificationService notificationService,
            CancellationToken cancellationToken = default)
        {
            var response = await notificationService.GetStatusAsync(currentUser.UserId, cancellationToken);
            return response.ToHttpResult();
        }

        private static async Task<IResult> MarkAllNotificationsAsRead(
            ICurrentUserService currentUser,
            INotificationService notificationService,
            CancellationToken cancellationToken = default)
        {
            var response = await notificationService.MarkAllAsReadAsync(currentUser.UserId, cancellationToken);
            return response.ToHttpResult();
        }

        private static async Task<IResult> MarkOneNotificationAsRead(
            Guid id,
            ICurrentUserService currentUser,
            INotificationService notificationService,
            CancellationToken cancellationToken = default)
        {
            var response = await notificationService.MarkAsReadAsync(currentUser.UserId, id, cancellationToken);
            return response.ToHttpResult();
        }

        private static async Task<IResult> SoftDeleteNotificationById(
            Guid id,
            ICurrentUserService currentUser,
            INotificationService notificationService,
            CancellationToken cancellationToken = default)
        {
            var response = await notificationService.SoftDeleteByIdAsync(currentUser.UserId, id, cancellationToken);
            return response.ToHttpResult();
        }
    }
}
