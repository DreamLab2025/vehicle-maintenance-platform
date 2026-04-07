using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Mappings;

public static class BookingMappings
{
    public static BookingResponse ToResponse(
        this Booking booking,
        BookingCustomerSummary? customer = null,
        BookingVehicleSummary? vehicleSnapshot = null,
        bool includeFullVehicle = false,
        IReadOnlyDictionary<Guid, string>? changedByNames = null)
    {
        var branch = booking.GarageBranch;
        var garage = branch.Garage;

        return new BookingResponse
        {
            Id = booking.Id,
            CustomerName = customer?.FullName ?? string.Empty,
            CustomerPhone = customer?.PhoneNumber ?? string.Empty,
            VehicleBrand = vehicleSnapshot?.BrandName ?? string.Empty,
            VehicleModel = vehicleSnapshot?.ModelName ?? string.Empty,
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
                    ChangedByName = changedByNames?.TryGetValue(h.ChangedByUserId, out var n) == true ? n : null,
                    Note = h.Note,
                    ChangedAt = h.ChangedAt,
                    CreatedAt = h.CreatedAt
                })
                .ToList(),
            Customer = customer,
            Vehicle = includeFullVehicle ? vehicleSnapshot : null
        };
    }

    public static BookingListItemResponse ToListItemResponse(this Booking booking, string? itemsSummaryOverride = null)
    {
        var itemsSummary = itemsSummaryOverride ?? BuildItemsSummary(booking);

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

    public static string BuildItemsSummaryFromLineItems(IReadOnlyList<BookingLineItem> items)
    {
        var ordered = items.OrderBy(i => i.SortOrder).ToList();
        if (ordered.Count == 0)
            return string.Empty;
        if (ordered.Count == 1)
            return ordered[0].Product?.Name ?? ordered[0].Service?.Name ?? ordered[0].Bundle?.Name ?? string.Empty;

        var firstName = ordered[0].Product?.Name ?? ordered[0].Service?.Name ?? ordered[0].Bundle?.Name ?? string.Empty;
        return $"{firstName} và {ordered.Count - 1} mục khác";
    }

    private static BookingLineItemResponse ToLineItemResponse(this BookingLineItem item)
    {
        var name = item.Product?.Name ?? item.Service?.Name ?? item.Bundle?.Name ?? string.Empty;

        BookingProductSummary? productDetails = null;
        if (item.Product is not null)
        {
            productDetails = new BookingProductSummary
            {
                Id = item.Product.Id,
                Name = item.Product.Name,
                Description = item.Product.Description,
                ImageUrl = item.Product.ImageUrl,
                MaterialPrice = item.Product.MaterialPrice.Amount,
                MaterialPriceCurrency = item.Product.MaterialPrice.Currency,
                EstimatedDurationMinutes = item.Product.EstimatedDurationMinutes
            };
        }

        BookingServiceSummary? serviceDetails = null;
        if (item.Service is not null)
        {
            serviceDetails = new BookingServiceSummary
            {
                Id = item.Service.Id,
                Name = item.Service.Name,
                Description = item.Service.Description,
                ImageUrl = item.Service.ImageUrl,
                LaborPrice = item.Service.LaborPrice.Amount,
                LaborPriceCurrency = item.Service.LaborPrice.Currency,
                EstimatedDurationMinutes = item.Service.EstimatedDurationMinutes
            };
        }

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
            ProductDetails = productDetails,
            ServiceDetails = serviceDetails,
            BundleDetails = bundleDetails
        };
    }

    private static string BuildItemsSummary(Booking booking)
    {
        var items = booking.LineItems.OrderBy(i => i.SortOrder).ToList();
        return BuildItemsSummaryFromLineItems(items);
    }

    private static string FormatAddressLine(GarageBranch branch) =>
        branch.Address.StreetDetail.Trim();
}
