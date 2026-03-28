using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Services.Interfaces;

public interface IBookingService
{
    Task<ApiResponse<BookingResponse>> CreateBookingAsync(
        Guid userId,
        CreateBookingRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<BookingResponse>> GetBookingByIdAsync(
        Guid bookingId,
        Guid viewerId,
        CancellationToken ct = default);

    Task<bool> CanViewBookingAsync(Guid bookingId, Guid viewerId, CancellationToken ct = default);

    Task<ApiResponse<List<BookingListItemResponse>>> GetBookingsAsync(
        Guid currentUserId,
        bool assignedToMe,
        Guid? branchId,
        Guid? userId,
        PaginationRequest pagination,
        CancellationToken ct = default);

    Task<ApiResponse<BookingResponse>> AssignMechanicAsync(
        Guid bookingId,
        Guid actorId,
        AssignBookingRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<BookingResponse>> UpdateMechanicStatusAsync(
        Guid bookingId,
        Guid mechanicUserId,
        UpdateBookingMechanicStatusRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<bool>> CancelBookingAsync(
        Guid bookingId,
        Guid actorId,
        string? reason,
        CancellationToken ct = default);
}
