using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Mappings;

public static class BookingMappings
{
    public static BookingResponse ToResponse(
        this Booking booking,
        BookingCustomerSummary? customer = null,
        BookingVehicleSummary? vehicle = null)
    {
        var branch = booking.GarageBranch;
        var garage = branch.Garage;

        return new BookingResponse
        {
            Id = booking.Id,
            UserId = booking.UserId,
            UserVehicleId = booking.UserVehicleId,
            GarageBranchId = booking.GarageBranchId,
            MechanicId = booking.MechanicId,
            MechanicDisplayName = booking.Mechanic?.DisplayName,
            Status = booking.Status,
            ScheduledAt = booking.ScheduledAt,
            Note = booking.Note,
            BookedTotalAmount = booking.BookedTotalPrice.Amount,
            BookedCurrency = booking.BookedTotalPrice.Currency,
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
            LineItems = booking.LineItems
                .OrderBy(i => i.SortOrder)
                .Select(i => i.ToLineItemResponse())
                .ToList(),
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
        var itemsSummary = BuildItemsSummary(booking);

        return new BookingListItemResponse
        {
            Id = booking.Id,
            Status = booking.Status,
            ScheduledAt = booking.ScheduledAt,
            GarageBranchId = booking.GarageBranchId,
            BranchName = booking.GarageBranch.Name,
            ItemsSummary = itemsSummary,
            BookedTotalAmount = booking.BookedTotalPrice.Amount,
            BookedCurrency = booking.BookedTotalPrice.Currency
        };
    }

    private static BookingLineItemResponse ToLineItemResponse(this BookingLineItem item)
    {
        var name = item.Product?.Name ?? item.Service?.Name ?? item.Bundle?.Name ?? string.Empty;

        BookingBundleSummary? bundleDetails = null;
        if (item.Bundle is not null)
        {
            bundleDetails = new BookingBundleSummary
            {
                Id = item.Bundle.Id,
                Name = item.Bundle.Name,
                DiscountAmount = item.Bundle.DiscountAmount?.Amount,
                DiscountPercent = item.Bundle.DiscountPercent,
                Items = item.Bundle.Items
                    .OrderBy(bi => bi.SortOrder)
                    .Select(bi => new BookingBundleItemSummary
                    {
                        ProductId = bi.ProductId,
                        ServiceId = bi.ServiceId,
                        ItemName = bi.Product?.Name ?? bi.Service?.Name ?? string.Empty,
                        IncludeInstallation = bi.IncludeInstallation
                    })
                    .ToList()
            };
        }

        return new BookingLineItemResponse
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ServiceId = item.ServiceId,
            BundleId = item.BundleId,
            IncludeInstallation = item.IncludeInstallation,
            ItemName = name,
            BookedItemAmount = item.BookedItemPrice.Amount,
            BookedItemCurrency = item.BookedItemPrice.Currency,
            SortOrder = item.SortOrder,
            BundleDetails = bundleDetails
        };
    }

    private static string BuildItemsSummary(Booking booking)
    {
        var items = booking.LineItems.OrderBy(i => i.SortOrder).ToList();
        if (items.Count == 0)
            return string.Empty;

        if (items.Count == 1)
            return items[0].Product?.Name ?? items[0].Service?.Name ?? items[0].Bundle?.Name ?? string.Empty;

        var firstName = items[0].Product?.Name ?? items[0].Service?.Name ?? items[0].Bundle?.Name ?? string.Empty;
        return $"{firstName} và {items.Count - 1} mục khác";
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
