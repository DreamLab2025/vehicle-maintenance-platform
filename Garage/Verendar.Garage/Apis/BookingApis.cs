using Microsoft.AspNetCore.Mvc;
using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Application.Services.Interfaces;

namespace Verendar.Garage.Apis;

public static class BookingApis
{
    public static IEndpointRouteBuilder MapBookingApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/bookings")
            .MapBookingRoutes()
            .WithTags("Booking Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    public static RouteGroupBuilder MapBookingRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateBooking)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<CreateBookingRequest>())
            .WithName("CreateBooking")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Người dùng tạo đặt lịch (MVP — chưa thanh toán)";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<BookingResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<BookingResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/", GetBookings)
            .WithName("GetBookings")
            .WithOpenApi(operation =>
            {
                operation.Summary =
                    "Danh sách booking: userId (user), branchId (Owner/Manager), assignedToMe=true (Mechanic)";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<List<BookingListItemResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<BookingListItemResponse>>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<List<BookingListItemResponse>>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<List<BookingListItemResponse>>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/{id:guid}/assign", AssignMechanic)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<AssignBookingRequest>())
            .WithName("AssignBookingMechanic")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Owner/Manager gán thợ máy (Pending/AwaitingConfirmation → Confirmed)";
                return operation;
            })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.GarageOwner), nameof(RoleType.GarageManager)))
            .Produces<ApiResponse<BookingResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<BookingResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<BookingResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<BookingResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/{id:guid}/status", UpdateMechanicStatus)
            .AddEndpointFilter(ValidationEndpointFilter.Validate<UpdateBookingMechanicStatusRequest>())
            .WithName("UpdateBookingMechanicStatus")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Thợ được gán cập nhật tiến độ (Confirmed → InProgress → Completed)";
                return operation;
            })
            .RequireAuthorization(policy => policy.RequireRole(nameof(RoleType.Mechanic)))
            .Produces<ApiResponse<BookingResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<BookingResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<BookingResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<BookingResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{id:guid}", CancelBooking)
            .WithName("CancelBooking")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Hủy booking (chủ booking hoặc Owner/Manager chi nhánh)";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<bool>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetBookingById)
            .WithName("GetBookingById")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Chi tiết booking (user / Owner / Manager / thợ được gán — thợ có thêm khách & xe)";
                return operation;
            })
            .RequireAuthorization()
            .Produces<ApiResponse<BookingResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<BookingResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<BookingResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> CreateBooking(
        [FromBody] CreateBookingRequest request,
        ICurrentUserService currentUserService,
        IBookingService bookingService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await bookingService.CreateBookingAsync(userId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetBookings(
        [AsParameters] GetBookingsRequest request,
        ICurrentUserService currentUserService,
        IBookingService bookingService,
        CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        if (currentUserId == Guid.Empty)
            return Results.Unauthorized();

        var result = await bookingService.GetBookingsAsync(
            currentUserId, request.AssignedToMe == true, request.BranchId, request.UserId, request.Status, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetBookingById(
        [FromRoute] Guid id,
        ICurrentUserService currentUserService,
        IBookingService bookingService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await bookingService.GetBookingByIdAsync(id, userId, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> AssignMechanic(
        [FromRoute] Guid id,
        [FromBody] AssignBookingRequest request,
        ICurrentUserService currentUserService,
        IBookingService bookingService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await bookingService.AssignMechanicAsync(id, userId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateMechanicStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateBookingMechanicStatusRequest request,
        ICurrentUserService currentUserService,
        IBookingService bookingService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await bookingService.UpdateMechanicStatusAsync(id, userId, request, ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CancelBooking(
        [FromRoute] Guid id,
        [FromQuery] string? reason,
        ICurrentUserService currentUserService,
        IBookingService bookingService,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await bookingService.CancelBookingAsync(id, userId, reason, ct);
        return result.ToHttpResult();
    }
}
