using Verendar.Common.Jwt;
using Verendar.Common.Shared;
using Verendar.Notification.Application.Dtos.Notifications;
using Verendar.Notification.Application.Services.Interfaces;

namespace Verendar.Notification.Apis;

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
                operation.Summary = "Lấy danh sách thông báo của người dùng (có phân trang)";
                return operation;
            })
            .Produces<ApiResponse<List<NotificationListItemDto>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetNotificationDetailForUser)
            .WithName("GetNotificationDetailForUser")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Lấy chi tiết thông báo (kèm metadata snapshot lúc gửi)";
                return operation;
            })
            .Produces<ApiResponse<NotificationDetailDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return builder;
    }

    private static async Task<IResult> GetAllNotificationsForUser(
        ICurrentUserService currentUser,
        INotificationService notificationService,
        [AsParameters] PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var response = await notificationService.GetInAppNotificationsForUserAsync(userId, request, cancellationToken);
        return response.IsSuccess ? Results.Ok(response) : Results.BadRequest(response);
    }

    private static async Task<IResult> GetNotificationDetailForUser(
        Guid id,
        ICurrentUserService currentUser,
        INotificationService notificationService,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var response = await notificationService.GetNotificationDetailForUserAsync(userId, id, cancellationToken);
        return response.IsSuccess ? Results.Ok(response) : Results.NotFound(response);
    }
}
