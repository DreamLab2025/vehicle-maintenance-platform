using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Mappings;

public static class BookingMappings
{
    public static BookingProductSummary ToBookingProductSummary(this GarageProduct product) =>
        new()
        {
            Id = product.Id,
            Name = product.Name,
            Type = product.Type,
            PartCategoryId = product.PartCategoryId,
            EstimatedDurationMinutes = product.EstimatedDurationMinutes
        };

    public static BookingResponse ToResponse(
        this Booking booking,
        BookingCustomerSummary? customer = null,
        BookingVehicleSummary? vehicle = null)
    {
        var branch = booking.GarageBranch;
        var garage = branch.Garage;
        var product = booking.GarageProduct;

        return new BookingResponse
        {
            Id = booking.Id,
            UserId = booking.UserId,
            UserVehicleId = booking.UserVehicleId,
            GarageBranchId = booking.GarageBranchId,
            GarageProductId = booking.GarageProductId,
            MechanicId = booking.MechanicId,
            MechanicDisplayName = booking.Mechanic?.DisplayName,
            Status = booking.Status,
            ScheduledAt = booking.ScheduledAt,
            Note = booking.Note,
            BookedAmount = booking.BookedPrice.Amount,
            BookedCurrency = booking.BookedPrice.Currency,
            CompletedAt = booking.CompletedAt,
            CurrentOdometer = booking.CurrentOdometer,
            CancellationReason = booking.CancellationReason,
            PaymentId = booking.PaymentId,
            Branch = new BookingBranchSummary
            {
                Id = branch.Id,
                Name = branch.Name,
                AddressLine = FormatAddressLine(branch),
                GarageId = garage.Id,
                GarageBusinessName = garage.BusinessName
            },
            Product = product.ToBookingProductSummary(),
            StatusHistory = booking.StatusHistory
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new BookingStatusHistoryItemResponse
                {
                    Id = h.Id,
                    FromStatus = h.FromStatus,
                    ToStatus = h.ToStatus,
                    ChangedByUserId = h.ChangedByUserId,
                    Note = h.Note,
                    ChangedAt = h.ChangedAt,
                    CreatedAt = h.CreatedAt
                })
                .ToList(),
            Customer = customer,
            Vehicle = vehicle
        };
    }

    public static BookingListItemResponse ToListItemResponse(this Booking booking)
    {
        return new BookingListItemResponse
        {
            Id = booking.Id,
            Status = booking.Status,
            ScheduledAt = booking.ScheduledAt,
            GarageBranchId = booking.GarageBranchId,
            BranchName = booking.GarageBranch.Name,
            ProductName = booking.GarageProduct.Name,
            BookedAmount = booking.BookedPrice.Amount,
            BookedCurrency = booking.BookedPrice.Currency
        };
    }

    private static string FormatAddressLine(GarageBranch branch)
    {
        var a = branch.Address;
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(a.HouseNumber))
            parts.Add(a.HouseNumber.Trim());
        if (!string.IsNullOrWhiteSpace(a.StreetDetail))
            parts.Add(a.StreetDetail.Trim());
        return string.Join(", ", parts);
    }
}
