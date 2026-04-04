namespace Verendar.Garage.Domain.Models;

public sealed record BookingAssignmentSnapshot(
    Guid Id,
    BookingStatus Status,
    Guid GarageBranchId,
    Guid UserId,
    DateTime ScheduledAt);
